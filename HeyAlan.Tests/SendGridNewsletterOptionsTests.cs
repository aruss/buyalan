namespace HeyAlan.Tests;

using HeyAlan.Newsletter;
using Microsoft.Extensions.Configuration;

public class SendGridNewsletterOptionsTests
{
    [Fact]
    public void TryGetSendGridNewsletterOptions_WhenKeysExist_ReturnsTrimmedValues()
    {
        Dictionary<string, string?> values = new()
        {
            ["SENDGRID_API_KEY"] = "  api-key-value  ",
            ["SENDGRID_NEWSLETTER_LIST_ID"] = "  list-id-value  ",
            ["SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID"] = "  d-123  ",
            ["SENDGRID_NEWSLETTER_FROM_EMAIL"] = "  newsletter@example.com  ",
            ["NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES"] = "  60  "
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        SendGridOptions options = configuration.TryGetSendGridOptions();

        Assert.Equal("api-key-value", options.ApiKey);
        Assert.Equal("list-id-value", options.NewsletterListId);
        Assert.Equal("d-123", options.NewsletterConfirmTemplateId);
        Assert.Equal("newsletter@example.com", options.NewsletterFromEmail);
        Assert.Equal(60, options.ConfirmTokenTtlMinutes);
    }

    [Fact]
    public void TryGetSendGridNewsletterOptions_WhenApiKeyMissing_Throws()
    {
        Dictionary<string, string?> values = new()
        {
            ["SENDGRID_NEWSLETTER_LIST_ID"] = "list-id-value",
            ["SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID"] = "d-123",
            ["SENDGRID_NEWSLETTER_FROM_EMAIL"] = "newsletter@example.com"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => configuration.TryGetSendGridOptions());

        Assert.Contains("SENDGRID_API_KEY", exception.Message);
    }

    [Fact]
    public void TryGetSendGridNewsletterOptions_WhenListIdMissing_Throws()
    {
        Dictionary<string, string?> values = new()
        {
            ["SENDGRID_API_KEY"] = "api-key-value",
            ["SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID"] = "d-123",
            ["SENDGRID_NEWSLETTER_FROM_EMAIL"] = "newsletter@example.com"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => configuration.TryGetSendGridOptions());

        Assert.Contains("SENDGRID_NEWSLETTER_LIST_ID", exception.Message);
    }

    [Fact]
    public void TryGetSendGridNewsletterOptions_WhenTemplateMissing_Throws()
    {
        Dictionary<string, string?> values = new()
        {
            ["SENDGRID_API_KEY"] = "api-key-value",
            ["SENDGRID_NEWSLETTER_LIST_ID"] = "list-id-value",
            ["SENDGRID_NEWSLETTER_FROM_EMAIL"] = "newsletter@example.com"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => configuration.TryGetSendGridOptions());

        Assert.Contains("SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID", exception.Message);
    }

    [Fact]
    public void TryGetSendGridNewsletterOptions_WhenFromEmailMissing_Throws()
    {
        Dictionary<string, string?> values = new()
        {
            ["SENDGRID_API_KEY"] = "api-key-value",
            ["SENDGRID_NEWSLETTER_LIST_ID"] = "list-id-value",
            ["SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID"] = "d-template-id"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => configuration.TryGetSendGridOptions());

        Assert.Contains("SENDGRID_NEWSLETTER_FROM_EMAIL", exception.Message);
    }

    [Fact]
    public void TryGetSendGridNewsletterOptions_WhenTokenTtlInvalid_Throws()
    {
        Dictionary<string, string?> values = new()
        {
            ["SENDGRID_API_KEY"] = "api-key-value",
            ["SENDGRID_NEWSLETTER_LIST_ID"] = "list-id-value",
            ["SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID"] = "d-template-id",
            ["SENDGRID_NEWSLETTER_FROM_EMAIL"] = "newsletter@example.com",
            ["NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES"] = "0"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => configuration.TryGetSendGridOptions());

        Assert.Contains("NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES", exception.Message);
    }
}
