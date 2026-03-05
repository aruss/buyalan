namespace HeyAlan.WebApi.Newsletter;

using Microsoft.AspNetCore.Mvc;
using HeyAlan.Newsletter;
using Wolverine;
using System.Net.Mail;

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
        IMessageBus messageBus,
        CancellationToken ct)
    {
        if (!TryNormalizeEmail(input.Email, out string normalizedEmail))
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid email",
                detail: "A valid email address is required.");
        }

        NewsletterSubscriptionRequested message = new(
            normalizedEmail,
            DateTimeOffset.UtcNow);

        await messageBus.PublishAsync(message);

        return TypedResults.Ok(new CreateNewsletterSubscriptionResult(true));
    }

    private static async Task<IResult> ConfirmNewsletterSubscriptionAsync(
        [FromBody] ConfirmNewsletterSubscriptionInput input,
        INewsletterConfirmationTokenService confirmationTokenService,
        ISendGridClient sendGridClient,
        CancellationToken ct)
    {
        string token = input.Token ?? String.Empty;

        bool hasValidToken = confirmationTokenService
            .TryReadEmail(token, out string confirmedEmail);

        if (!hasValidToken)
        {
            return TypedResults.Ok(new ConfirmNewsletterSubscriptionResult(true));
        }

        await sendGridClient.UpsertNewsletterContactAsync(confirmedEmail, ct);
        return TypedResults.Ok(new ConfirmNewsletterSubscriptionResult(true));
    }

    private static bool TryNormalizeEmail(string? value, out string normalizedEmail)
    {
        normalizedEmail = String.Empty;

        if (String.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string trimmed = value.Trim();
        try
        {
            MailAddress parsed = new(trimmed);

            if (!String.Equals(parsed.Address, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            normalizedEmail = parsed.Address;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
