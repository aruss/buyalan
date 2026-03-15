namespace BuyAlan.WebApi.Newsletter;

using BuyAlan;
using BuyAlan.Configuration;
using BuyAlan.Email;
using BuyAlan.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

public static class NewsletterEndpoints
{
    public static IEndpointRouteBuilder MapNewsletterEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        RouteGroupBuilder routeGroup = routeBuilder
            .MapGroup("/newsletter")
            .WithTags("Newsletter");

        routeGroup
            .MapPost(
                "/subscribe",
                CreateNewsletterSubscriptionAsync)
            .AllowAnonymous()
            .Produces<CreateNewsletterSubscriptionResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        routeGroup
            .MapPost(
                "/confirm",
                ConfirmNewsletterSubscriptionAsync)
            .AllowAnonymous()
            .Produces<ConfirmNewsletterSubscriptionResult>(StatusCodes.Status200OK);

        return routeBuilder;
    }

    private static async Task<IResult> CreateNewsletterSubscriptionAsync(
        [FromBody] CreateNewsletterSubscriptionInput input,
        IEmailQueuingService emailService,
        AppOptions appOptions,
        INewsletterConfirmationTokenService confirmationTokenService,
        CancellationToken ct)
    {
        if (!input.Email.TryNormalizeEmail(out string normalizedEmail))
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid email",
                detail: "A valid email address is required.");
        }
        
        string emailHash = ComputeSha256(normalizedEmail);

        string token = confirmationTokenService
            .CreateToken(normalizedEmail, DateTimeOffset.UtcNow);

        string confirmationUrl = BuildConfirmationUrl(appOptions.PublicBaseUrl, token);

        EmailSendRequested emailMessage = new(
            normalizedEmail,
            EmailTemplateKey.NewsletterConfirmation,
            new Dictionary<string, string>
            {
                ["confirmation_url"] = confirmationUrl
            });

        await emailService.EnqueueAsync(emailMessage, ct);

        return TypedResults.Ok(new CreateNewsletterSubscriptionResult(true));
    }

    private static string BuildConfirmationUrl(Uri publicBaseUrl, string token)
    {
        string trimmedBaseUrl = publicBaseUrl.AbsoluteUri.TrimEnd('/');
        string confirmPath = $"{trimmedBaseUrl}/newsletter/confirm";
        return QueryHelpers.AddQueryString(confirmPath, "token", token);
    }

    private static string ComputeSha256(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }

    private static async Task<IResult> ConfirmNewsletterSubscriptionAsync(
        [FromBody] ConfirmNewsletterSubscriptionInput input,
        INewsletterConfirmationTokenService confirmationTokenService,
        INewsletterUpsertService newsletterUpsertService,
        CancellationToken ct)
    {
        string token = input.Token ?? String.Empty;

        bool hasValidToken = confirmationTokenService
            .TryReadEmail(token, out string confirmedEmail);

        if (!hasValidToken)
        {
            return TypedResults.Ok(new ConfirmNewsletterSubscriptionResult(true));
        }

        await newsletterUpsertService.UpsertNewsletterContactAsync(confirmedEmail, ct);
        return TypedResults.Ok(new ConfirmNewsletterSubscriptionResult(true));
    }
}


