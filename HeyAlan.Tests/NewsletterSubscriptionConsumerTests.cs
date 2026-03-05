namespace HeyAlan.Tests;

using HeyAlan.Configuration;
using HeyAlan.Newsletter;
using Microsoft.Extensions.Logging.Abstractions;

public class NewsletterSubscriptionConsumerTests
{
    [Fact]
    public async Task Consume_WhenEmailExists_SendsConfirmationEmail()
    {
        RecordingSendGridNewsletterClient newsletterClient = new();
        StubNewsletterConfirmationTokenService tokenService = new();
        AppOptions appOptions = new()
        {
            PublicBaseUrl = new Uri("https://heyalan.app")
        };

        NewsletterSubscriptionConsumer consumer = new(
            newsletterClient,
            tokenService,
            appOptions,
            NullLogger<NewsletterSubscriptionConsumer>.Instance);

        NewsletterSubscriptionRequested message = new("person@example.com", DateTimeOffset.UtcNow);
        await consumer.Consume(message, CancellationToken.None);

        Assert.Equal("person@example.com", newsletterClient.LastConfirmationEmail);
        Assert.Equal("https://heyalan.app/newsletter/confirm?token=fixed-token", newsletterClient.LastConfirmationUrl);
    }

    [Fact]
    public async Task Consume_WhenEmailIsWhitespace_DoesNotSendConfirmationEmail()
    {
        RecordingSendGridNewsletterClient newsletterClient = new();
        StubNewsletterConfirmationTokenService tokenService = new();
        AppOptions appOptions = new()
        {
            PublicBaseUrl = new Uri("https://heyalan.app")
        };

        NewsletterSubscriptionConsumer consumer = new(
            newsletterClient,
            tokenService,
            appOptions,
            NullLogger<NewsletterSubscriptionConsumer>.Instance);

        NewsletterSubscriptionRequested message = new("   ", DateTimeOffset.UtcNow);
        await consumer.Consume(message, CancellationToken.None);

        Assert.Null(newsletterClient.LastConfirmationEmail);
    }

    private sealed class RecordingSendGridNewsletterClient : ISendGridClient
    {
        public string? LastUpsertEmail { get; private set; }

        public Task UpsertNewsletterContactAsync(string email, CancellationToken cancellationToken = default)
        {
            this.LastUpsertEmail = email;
            return Task.CompletedTask;
        }

        public string? LastConfirmationEmail { get; private set; }

        public string? LastConfirmationUrl { get; private set; }

        public Task SendNewsletterConfirmationEmailAsync(string email, string confirmationUrl, CancellationToken cancellationToken = default)
        {
            this.LastConfirmationEmail = email;
            this.LastConfirmationUrl = confirmationUrl;
            return Task.CompletedTask;
        }
    }

    private sealed class StubNewsletterConfirmationTokenService : INewsletterConfirmationTokenService
    {
        public string CreateToken(string email, DateTimeOffset issuedAtUtc)
        {
            return "fixed-token";
        }

        public bool TryReadEmail(string token, out string email)
        {
            email = String.Empty;
            return false;
        }
    }
}
