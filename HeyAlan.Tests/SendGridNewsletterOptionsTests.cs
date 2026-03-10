namespace HeyAlan.Tests;

using HeyAlan.SendGridIntegration;
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
            ["SENDGRID_EMAIL_FROM"] = "  notifications@example.com  ",
            ["SENDGRID_NEWSLETTER_LIST_ID"] = "  list-id-value  ",
            ["SENDGRID_TEMPLATE_GENERIC"] = "  d-generic  ",
            ["SENDGRID_TEMPLATE_IDENTITY_CONFIRMATION_LINK"] = "  d-confirm  ",
            ["SENDGRID_TEMPLATE_IDENTITY_PASSWORD_RESET_LINK"] = "  d-reset-link  ",
            ["SENDGRID_TEMPLATE_IDENTITY_PASSWORD_RESET_CODE"] = "  d-reset-code  ",
            ["SENDGRID_TEMPLATE_NEWSLETTER_CONFIRMATION"] = "  d-newsletter  ",
            ["NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES"] = "  60  "
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        SendGridOptions options = configuration.TryGetSendGridOptions();

        Assert.Equal("api-key-value", options.ApiKey);
        Assert.Equal("list-id-value", options.NewsletterListId);
    }

    [Fact]
    public void TryGetSendGridNewsletterOptions_WhenApiKeyMissing_Throws()
    {
        Dictionary<string, string?> values = new()
        {
            ["SENDGRID_EMAIL_FROM"] = "notifications@example.com",
            ["SENDGRID_NEWSLETTER_LIST_ID"] = "list-id-value",
            ["SENDGRID_TEMPLATE_GENERIC"] = "d-generic",
            ["SENDGRID_TEMPLATE_IDENTITY_CONFIRMATION_LINK"] = "d-confirm",
            ["SENDGRID_TEMPLATE_IDENTITY_PASSWORD_RESET_LINK"] = "d-reset-link",
            ["SENDGRID_TEMPLATE_IDENTITY_PASSWORD_RESET_CODE"] = "d-reset-code",
            ["SENDGRID_TEMPLATE_NEWSLETTER_CONFIRMATION"] = "d-newsletter"
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
            ["SENDGRID_EMAIL_FROM"] = "notifications@example.com",
            ["SENDGRID_TEMPLATE_GENERIC"] = "d-generic",
            ["SENDGRID_TEMPLATE_IDENTITY_CONFIRMATION_LINK"] = "d-confirm",
            ["SENDGRID_TEMPLATE_IDENTITY_PASSWORD_RESET_LINK"] = "d-reset-link",
            ["SENDGRID_TEMPLATE_IDENTITY_PASSWORD_RESET_CODE"] = "d-reset-code",
            ["SENDGRID_TEMPLATE_NEWSLETTER_CONFIRMATION"] = "d-newsletter"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => configuration.TryGetSendGridOptions());

        Assert.Contains("SENDGRID_NEWSLETTER_LIST_ID", exception.Message);
    }

    [Fact]
    public void TryGetNewsletterOptions_WhenTokenTtlProvided_ReturnsParsedValue()
    {
        Dictionary<string, string?> values = new()
        {
            ["NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES"] = "60"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        NewsletterOptions options = configuration.TryGetNewsletterOptions();

        Assert.Equal(60, options.ConfirmTokenTtlMinutes);
    }

    [Fact]
    public void TryGetNewsletterOptions_WhenTokenTtlInvalid_Throws()
    {
        Dictionary<string, string?> values = new()
        {
            ["NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES"] = "0"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => configuration.TryGetNewsletterOptions());

        Assert.Contains("NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES", exception.Message);
    }
}
