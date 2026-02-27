namespace ShelfBuddy.WebApi.Identity;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShelfBuddy.Configuration;
using ShelfBuddy.Data;
using ShelfBuddy.Data.Entities;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

public static class IdentityBuilderExtensions
{
    public static TBuilder AddIdentityServices<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        AppOptions appOptions = builder.Configuration.TryGetAppOptions();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<MainDataContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddTransient<IEmailSender<ApplicationUser>, LoggingEmailSender>();

        var authBuilder = builder.Services
            .AddAuthentication(IdentityConstants.ApplicationScheme);            

        authBuilder.AddIdentityCookies(); 

        if (!String.IsNullOrWhiteSpace(appOptions.GoogleClientId) &&
            !String.IsNullOrWhiteSpace(appOptions.GoogleClientSecret))
        {
            authBuilder.AddGoogle("Google", "Google", options =>
            {
                options.ClientId = appOptions.GoogleClientId;
                options.ClientSecret = appOptions.GoogleClientSecret;
                options.CallbackPath = "/auth/signin-google";
                options.SignInScheme = IdentityConstants.ExternalScheme;
            });
        }

        if (!string.IsNullOrWhiteSpace(appOptions.SquareClientId) &&
            !string.IsNullOrWhiteSpace(appOptions.SquareClientSecret))
        {
            authBuilder.AddOAuth("Square", "Square", options =>
            {
                options.ClientId = appOptions.SquareClientId;
                options.ClientSecret = appOptions.SquareClientSecret;
                options.CallbackPath = "/auth/signin-square";
                options.SignInScheme = IdentityConstants.ExternalScheme;

                options.AuthorizationEndpoint = "https://connect.squareup.com/oauth2/authorize";
                options.TokenEndpoint = "https://connect.squareup.com/oauth2/token";
                options.UserInformationEndpoint = "https://connect.squareup.com/v2/merchants/me";

                options.Scope.Add("MERCHANT_PROFILE_READ");

                // Map Square JSON object to standard ASP.NET Core Identity Claims
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "main_email");
                options.ClaimActions.MapJsonKey("name", "business_name");

                options.Events.OnCreatingTicket = async context =>
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                    // Square requires an explicit API version header
                    request.Headers.Add("Square-Version", "2024-01-18");

                    using var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();

                    using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(context.HttpContext.RequestAborted));
                    JsonElement merchant = document.RootElement.GetProperty("merchant");

                    context.RunClaimActions(merchant);

                    // Synthesize the email_verified claim to satisfy existing IsExternalEmailVerified validation in IdentityEndpoints
                    if (merchant.TryGetProperty("main_email", out _))
                    {
                        context.Identity?.AddClaim(new Claim("email_verified", "true"));
                    }
                };
            });
        }

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("OnboardedOnly", p => p.RequireClaim("onboarded", "true"));
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        // for development phase use simple passwords
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 1;
                options.Password.RequiredUniqueChars = 0;
            });
        }

        return builder;
    }
}
