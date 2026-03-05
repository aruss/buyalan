namespace HeyAlan.Newsletter;

using HeyAlan.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

public sealed class NewsletterSubscriptionConsumer
{
    private readonly ISendGridClient sendGridNewsletterClient;
    private readonly INewsletterConfirmationTokenService confirmationTokenService;
    private readonly AppOptions appOptions;
    private readonly ILogger<NewsletterSubscriptionConsumer> logger;

    public NewsletterSubscriptionConsumer(
        ISendGridClient sendGridNewsletterClient,
        INewsletterConfirmationTokenService confirmationTokenService,
        AppOptions appOptions,
        ILogger<NewsletterSubscriptionConsumer> logger)
    {
        this.sendGridNewsletterClient = sendGridNewsletterClient ?? 
            throw new ArgumentNullException(nameof(sendGridNewsletterClient));

        this.confirmationTokenService = confirmationTokenService ?? 
            throw new ArgumentNullException(nameof(confirmationTokenService));

        this.appOptions = appOptions ?? 
            throw new ArgumentNullException(nameof(appOptions));

        this.logger = logger ??
            throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(
        NewsletterSubscriptionRequested message,
        CancellationToken ct)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        string normalizedEmail = message.Email.Trim();
        if (normalizedEmail.Length == 0)
        {
            this.logger
                .LogWarning("Received newsletter message with empty email payload.");

            return;
        }

        string emailHash = ComputeSha256(normalizedEmail);

        string token = this.confirmationTokenService
            .CreateToken(normalizedEmail, message.RequestedAtUtc);

        string confirmationUrl = BuildConfirmationUrl(this.appOptions.PublicBaseUrl, token);

        await this.sendGridNewsletterClient
            .SendNewsletterConfirmationEmailAsync(normalizedEmail, confirmationUrl, ct);

        this.logger.LogInformation(
            "Processed newsletter DOI request for email hash {EmailHash}.",
            emailHash);
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
}
