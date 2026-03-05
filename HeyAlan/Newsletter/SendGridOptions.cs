namespace HeyAlan.Newsletter;

using Microsoft.Extensions.Configuration;
using HeyAlan.Configuration;
public sealed record SendGridOptions
{
    public string ApiKey { get; init; } = String.Empty;

    public string NewsletterListId { get; init; } = String.Empty;

    public string NewsletterConfirmTemplateId { get; init; } = String.Empty;

    public string NewsletterFromEmail { get; init; } = String.Empty;

    public int ConfirmTokenTtlMinutes { get; init; } = 24 * 60;
}

public static class SendGridOptionsConfigurationExtensions
{
    public static SendGridOptions TryGetSendGridOptions(this IConfiguration configuration)
    {
        string apiKey = configuration["SENDGRID_API_KEY"]
            ?? throw ConfigurationErrors.Missing("SENDGRID_API_KEY");

        string listId = configuration["SENDGRID_NEWSLETTER_LIST_ID"]
            ?? throw ConfigurationErrors.Missing("SENDGRID_NEWSLETTER_LIST_ID");

        string confirmationTemplateId = configuration["SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID"]
            ?? throw ConfigurationErrors.Missing("SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID");

        string fromEmail = configuration["SENDGRID_NEWSLETTER_FROM_EMAIL"]
            ?? throw ConfigurationErrors.Missing("SENDGRID_NEWSLETTER_FROM_EMAIL");

        string? tokenTtlMinutesRaw = configuration["NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES"];

        string normalizedApiKey = apiKey.Trim();
        if (String.IsNullOrWhiteSpace(normalizedApiKey))
        {
            throw ConfigurationErrors.Invalid("SENDGRID_API_KEY");
        }

        string normalizedListId = listId.Trim();
        if (String.IsNullOrWhiteSpace(normalizedListId))
        {
            throw ConfigurationErrors.Invalid("SENDGRID_NEWSLETTER_LIST_ID");
        }

        string normalizedTemplateId = confirmationTemplateId.Trim();
        if (String.IsNullOrWhiteSpace(normalizedTemplateId))
        {
            throw ConfigurationErrors.Invalid("SENDGRID_NEWSLETTER_CONFIRM_TEMPLATE_ID");
        }

        string normalizedFromEmail = fromEmail.Trim();
        if (String.IsNullOrWhiteSpace(normalizedFromEmail))
        {
            throw ConfigurationErrors.Invalid("SENDGRID_NEWSLETTER_FROM_EMAIL");
        }

        int tokenTtlMinutes = 24 * 60;
        if (!String.IsNullOrWhiteSpace(tokenTtlMinutesRaw))
        {
            string normalizedTokenTtlMinutesRaw = tokenTtlMinutesRaw.Trim();
            if (!int.TryParse(normalizedTokenTtlMinutesRaw, out tokenTtlMinutes) || tokenTtlMinutes <= 0)
            {
                throw ConfigurationErrors.Invalid("NEWSLETTER_CONFIRM_TOKEN_TTL_MINUTES");
            }
        }

        return new SendGridOptions
        {
            ApiKey = normalizedApiKey,
            NewsletterListId = normalizedListId,
            NewsletterConfirmTemplateId = normalizedTemplateId,
            NewsletterFromEmail = normalizedFromEmail,
            ConfirmTokenTtlMinutes = tokenTtlMinutes
        };
    }
}
