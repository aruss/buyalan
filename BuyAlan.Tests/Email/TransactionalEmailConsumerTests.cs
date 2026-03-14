namespace BuyAlan.Tests;

using BuyAlan.Email;
using Microsoft.Extensions.Logging.Abstractions;

public class TransactionalEmailConsumerTests
{
    [Fact]
    public async Task Consume_WhenMessageValid_ForwardsTemplateAndPayload()
    {
        RecordingTransactionalEmailService emailService = new();
        TransactionalEmailConsumer consumer = new(
            emailService,
            NullLogger<TransactionalEmailConsumer>.Instance);

        EmailSendRequested message = new(
            "person@example.com",
            EmailTemplateKey.NewsletterConfirmation,
            new Dictionary<string, string>
            {
                ["confirmation_url"] = "https://buyalan.test/newsletter/confirm?token=abc"
            });

        await consumer.Consume(message, CancellationToken.None);

        Assert.Equal("person@example.com", emailService.LastRecipientEmail);
        Assert.Equal(EmailTemplateKey.NewsletterConfirmation, emailService.LastTemplateKey);
        Assert.Equal("https://buyalan.test/newsletter/confirm?token=abc", emailService.LastTemplateData!["confirmation_url"]);
    }

    [Fact]
    public async Task Consume_WhenTransportFails_Throws()
    {
        ThrowingTransactionalEmailService emailService = new();
        TransactionalEmailConsumer consumer = new(
            emailService,
            NullLogger<TransactionalEmailConsumer>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => consumer.Consume(
            new EmailSendRequested(
                "person@example.com",
                EmailTemplateKey.NewsletterConfirmation,
                new Dictionary<string, string>
                {
                    ["confirmation_url"] = "https://buyalan.test/newsletter/confirm?token=abc"
                }),
            CancellationToken.None));
    }

    private sealed class RecordingTransactionalEmailService : ITransactionalEmailService
    {
        public string? LastRecipientEmail { get; private set; }

        public string? LastTemplateKey { get; private set; }

        public IReadOnlyDictionary<string, string>? LastTemplateData { get; private set; }

        public Task SendTemplateAsync(
            string recipientEmail,
            string templateKey,
            IReadOnlyDictionary<string, string> templateData,
            CancellationToken cancellationToken = default)
        {
            this.LastRecipientEmail = recipientEmail;
            this.LastTemplateKey = templateKey;
            this.LastTemplateData = templateData;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingTransactionalEmailService : ITransactionalEmailService
    {
        public Task SendTemplateAsync(
            string recipientEmail,
            string templateId,
            IReadOnlyDictionary<string, string> templateData,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("send failed");
        }
    }
}
