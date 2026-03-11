namespace BuyAlan.Tests;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using BuyAlan.Identity;
using System.Text.Json;

public class IdentityBuilderExtensionsTests
{
    [Theory]
    [InlineData("https://buyalan.test", "/auth/providers/google/callback", "https://buyalan.test/api/auth/providers/google/callback")]
    [InlineData("https://buyalan.test/", "/auth/providers/google/callback", "https://buyalan.test/api/auth/providers/google/callback")]
    [InlineData("https://buyalan.test/tenant", "/auth/providers/google/callback", "https://buyalan.test/tenant/api/auth/providers/google/callback")]
    [InlineData("https://buyalan.test/tenant/", "/auth/providers/google/callback", "https://buyalan.test/tenant/api/auth/providers/google/callback")]
    public void BuildAbsoluteAuthCallbackUrl_ReturnsExpectedUrl(
        string publicBaseUrl,
        string callbackPath,
        string expected)
    {
        Uri baseUri = new(publicBaseUrl);

        string callbackUrl = IdentityBuilderExtensions.BuildAbsoluteAuthCallbackUrl(baseUri, callbackPath);

        Assert.Equal(expected, callbackUrl);
    }

    [Fact]
    public void BuildAbsoluteAuthCallbackUrl_WhenCallbackPathInvalid_ThrowsArgumentException()
    {
        Uri baseUri = new("https://buyalan.test");

        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            IdentityBuilderExtensions.BuildAbsoluteAuthCallbackUrl(baseUri, "auth/providers/google/callback"));

        Assert.Contains("Callback path must start with '/'", exception.Message);
    }

    [Fact]
    public void ReplaceQueryParameter_ReplacesExistingValue()
    {
        string authorizationUrl = "https://connect.squareup.com/oauth2/authorize?client_id=abc&redirect_uri=http%3A%2F%2Flocalhost%3A5000%2Fauth%2Fproviders%2Fsquare%2Fcallback&state=xyz";
        string rewritten = IdentityBuilderExtensions.ReplaceQueryParameter(
            authorizationUrl,
            "redirect_uri",
            "https://buyalan.test/api/auth/providers/square/callback");

        Uri rewrittenUri = new(rewritten);
        Dictionary<string, Microsoft.Extensions.Primitives.StringValues> query = QueryHelpers.ParseQuery(rewrittenUri.Query);

        Assert.Equal("abc", query["client_id"].ToString());
        Assert.Equal("xyz", query["state"].ToString());
        Assert.Equal("https://buyalan.test/api/auth/providers/square/callback", query["redirect_uri"].ToString());
    }

    [Theory]
    [InlineData(CookieSecurePolicy.SameAsRequest)]
    [InlineData(CookieSecurePolicy.Always)]
    public void ConfigureAuthCookie_SetsExpectedCookieShape(CookieSecurePolicy securePolicy)
    {
        CookieBuilder cookie = new();

        IdentityBuilderExtensions.ConfigureAuthCookie(cookie, securePolicy);

        Assert.True(cookie.HttpOnly);
        Assert.Equal("/", cookie.Path);
        Assert.Equal(SameSiteMode.Lax, cookie.SameSite);
        Assert.Equal(securePolicy, cookie.SecurePolicy);
    }

    [Theory]
    [InlineData("{\"verified_email\":true}", "verified_email", true, true)]
    [InlineData("{\"verified_email\":false}", "verified_email", true, false)]
    [InlineData("{\"verified_email\":\"true\"}", "verified_email", true, true)]
    [InlineData("{\"verified_email\":\"1\"}", "verified_email", true, true)]
    [InlineData("{\"verified_email\":\"yes\"}", "verified_email", true, true)]
    [InlineData("{\"verified_email\":\"false\"}", "verified_email", true, false)]
    [InlineData("{\"verified_email\":\"0\"}", "verified_email", true, false)]
    [InlineData("{\"verified_email\":\"no\"}", "verified_email", true, false)]
    [InlineData("{\"email_verified\":true}", "email_verified", true, true)]
    [InlineData("{\"email_verified\":\"unexpected\"}", "email_verified", false, false)]
    [InlineData("{}", "verified_email", false, false)]
    public void TryGetVerificationValue_ParsesExpectedValues(
        string json,
        string propertyName,
        bool expectedFound,
        bool expectedValue)
    {
        using JsonDocument jsonDocument = JsonDocument.Parse(json);
        JsonElement root = jsonDocument.RootElement;

        bool found = IdentityBuilderExtensions.TryGetVerificationValue(root, propertyName, out bool result);

        Assert.Equal(expectedFound, found);
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task AddIdentityServices_WhenSquareCredentialsMissing_DoesNotRegisterSquareScheme()
    {
        IServiceProvider services = BuildServices(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "https://buyalan.test"
        });

        IAuthenticationSchemeProvider schemeProvider = services.GetRequiredService<IAuthenticationSchemeProvider>();
        IEnumerable<AuthenticationScheme> allSchemes = await schemeProvider.GetAllSchemesAsync();

        Assert.DoesNotContain(allSchemes, scheme => String.Equals(scheme.Name, "square", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AddIdentityServices_WhenSquareCredentialsPresent_RegistersSquareSchemeWithExpectedCallbackAndScope()
    {
        IServiceProvider services = BuildServices(new Dictionary<string, string?>
        {
            ["PUBLIC_BASE_URL"] = "https://buyalan.test",
            ["AUTH_SQUARE_CLIENT_ID"] = "sandbox-client-id",
            ["AUTH_SQUARE_CLIENT_SECRET"] = "square-client-secret"
        });

        IAuthenticationSchemeProvider schemeProvider = services.GetRequiredService<IAuthenticationSchemeProvider>();
        IEnumerable<AuthenticationScheme> allSchemes = await schemeProvider.GetAllSchemesAsync();
        AuthenticationScheme squareScheme = Assert.Single(
            allSchemes,
            scheme => String.Equals(scheme.Name, "square", StringComparison.OrdinalIgnoreCase));

        Assert.Equal("Square", squareScheme.DisplayName);

        IOptionsMonitor<OAuthOptions> optionsMonitor = services.GetRequiredService<IOptionsMonitor<OAuthOptions>>();
        OAuthOptions squareOptions = optionsMonitor.Get("square");

        Assert.Equal("/auth/providers/square/callback", squareOptions.CallbackPath.Value);
        Assert.Contains("MERCHANT_PROFILE_READ", squareOptions.Scope);
    }

    private static IServiceProvider BuildServices(IDictionary<string, string?> configurationValues)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        builder.Configuration.AddInMemoryCollection(configurationValues);
        builder.AddIdentityServices();

        IHost host = builder.Build();
        return host.Services;
    }
}
