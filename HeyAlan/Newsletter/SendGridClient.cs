namespace HeyAlan.Newsletter;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public sealed class SendGridClient : ISendGridClient
{
    private const string SendGridClientName = "SendGridMarketingClient";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly SendGridOptions options;

    public SendGridClient(
        IHttpClientFactory httpClientFactory,
        SendGridOptions options)
    {
        this.httpClientFactory = httpClientFactory ??
            throw new ArgumentNullException(nameof(httpClientFactory));

        this.options = options ?? 
            throw new ArgumentNullException(nameof(options));
    }

    public async Task UpsertNewsletterContactAsync(
        string email, 
        CancellationToken cancellationToken = default)
    {
        if (String.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        HttpClient client = this.httpClientFactory.CreateClient(SendGridClientName);
        using HttpRequestMessage request = this.CreateUpsertRequest(email.Trim());

        using HttpResponseMessage response = 
            await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string responseBody = response.Content is null
            ? String.Empty
            : await response.Content.ReadAsStringAsync(cancellationToken);

        throw new InvalidOperationException(
            $"SendGrid contact upsert failed with status {(int)response.StatusCode}: {responseBody}");
    }

    public async Task SendNewsletterConfirmationEmailAsync(
        string email,
        string confirmationUrl,
        CancellationToken cancellationToken = default)
    {
        if (String.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (String.IsNullOrWhiteSpace(confirmationUrl))
        {
            throw new ArgumentException("Confirmation URL is required.", nameof(confirmationUrl));
        }

        HttpClient client = this.httpClientFactory.CreateClient(SendGridClientName);

        using HttpRequestMessage request = 
            this.CreateTransactionalConfirmationRequest(
                email.Trim(), 
                confirmationUrl.Trim());

        using HttpResponseMessage response = 
            await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string responseBody = response.Content is null
            ? String.Empty
            : await response.Content.ReadAsStringAsync(cancellationToken);

        throw new InvalidOperationException(
            $"SendGrid confirmation email failed with status {(int)response.StatusCode}: {responseBody}");
    }

    private HttpRequestMessage CreateUpsertRequest(string email)
    {
        SendGridUpsertContactsPayload payload = new(
            [this.options.NewsletterListId],
            [new SendGridContactPayload(email)]);

        string payloadJson = JsonSerializer.Serialize(payload);

        HttpRequestMessage request = 
            new(HttpMethod.Put, "/v3/marketing/contacts");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", this.options.ApiKey);

        request.Content =
            new StringContent(payloadJson, Encoding.UTF8, "application/json");

        return request;
    }

    private HttpRequestMessage CreateTransactionalConfirmationRequest(
        string email, 
        string confirmationUrl)
    {
        SendGridMailSendPayload payload = new(
            [new SendGridPersonalizationPayload(
                [new SendGridEmailAddressPayload(email)],
                new SendGridDynamicTemplateDataPayload(confirmationUrl))],
            new SendGridEmailAddressPayload(this.options.NewsletterFromEmail),
            this.options.NewsletterConfirmTemplateId);

        string payloadJson = JsonSerializer.Serialize(payload);

        HttpRequestMessage request = new(HttpMethod.Post, "/v3/mail/send");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", this.options.ApiKey);

        request.Content =
            new StringContent(payloadJson, Encoding.UTF8, "application/json");

        return request;
    }

    private sealed record SendGridUpsertContactsPayload(
        string[] list_ids,
        SendGridContactPayload[] contacts);

    private sealed record SendGridContactPayload(string email);

    private sealed record SendGridMailSendPayload(
        SendGridPersonalizationPayload[] personalizations,
        SendGridEmailAddressPayload from,
        string template_id);

    private sealed record SendGridPersonalizationPayload(
        SendGridEmailAddressPayload[] to,
        SendGridDynamicTemplateDataPayload dynamic_template_data);

    private sealed record SendGridDynamicTemplateDataPayload(
        string confirmation_url);

    private sealed record SendGridEmailAddressPayload(string email);
}
