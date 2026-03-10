namespace HeyAlan.Tests;

using HeyAlan.Email;
using HeyAlan.SendGridIntegration;

public class SendGridEmailTemplateCatalogTests
{
    [Fact]
    public void ResolveTemplateId_WhenTemplateKeyIsKnown_ReturnsConfiguredTemplateId()
    {
        SendGridOptions options = new()
        {
            ApiKey = "sendgrid-api-key",
            FromEmail = "notifications@heyalan.app",
            NewsletterListId = "newsletter-list-id",
            GenericTemplateId = "d-generic",
            IdentityConfirmationLinkTemplateId = "d-confirm",
            IdentityPasswordResetLinkTemplateId = "d-reset-link",
            IdentityPasswordResetCodeTemplateId = "d-reset-code",
            NewsletterConfirmationTemplateId = "d-newsletter"
        };

        SendGridTransactionalEmailService service = new(new FakeHttpClientFactory(), options);

        string templateId = service.ResolveTemplateId(EmailTemplateKey.IdentityPasswordResetCode);

        Assert.Equal("d-reset-code", templateId);
    }

    [Fact]
    public void ResolveTemplateId_WhenTemplateKeyIsUnknown_Throws()
    {
        SendGridOptions options = new()
        {
            ApiKey = "sendgrid-api-key",
            FromEmail = "notifications@heyalan.app",
            NewsletterListId = "newsletter-list-id",
            GenericTemplateId = "d-generic",
            IdentityConfirmationLinkTemplateId = "d-confirm",
            IdentityPasswordResetLinkTemplateId = "d-reset-link",
            IdentityPasswordResetCodeTemplateId = "d-reset-code",
            NewsletterConfirmationTemplateId = "d-newsletter"
        };

        SendGridTransactionalEmailService service = new(new FakeHttpClientFactory(), options);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            service.ResolveTemplateId("missing-template"));

        Assert.Contains("missing-template", exception.Message);
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient
            {
                BaseAddress = new Uri("https://api.sendgrid.com")
            };
        }
    }
}
