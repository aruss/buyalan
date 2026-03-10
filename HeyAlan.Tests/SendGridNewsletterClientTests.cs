namespace HeyAlan.Tests;

using HeyAlan.Newsletter;
using HeyAlan.SendGridIntegration;
using System.Net;
using System.Text;
using System.Text.Json;

public class SendGridNewsletterClientTests
{
    [Fact]
    public async Task UpsertNewsletterContactAsync_SendsExpectedPayload()
    {
        RecordingHandler handler = new((HttpRequestMessage _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        });

        FakeHttpClientFactory httpClientFactory = new(handler);
        SendGridOptions options = new()
        {
            ApiKey = "sendgrid-api-key",
            NewsletterListId = "newsletter-list-id"
        };

        SendGridNewsletterUpsertService client = new(httpClientFactory, options);
        await client.UpsertNewsletterContactAsync("person@example.com");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.Equal("https://api.sendgrid.com/v3/marketing/contacts", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization?.Scheme);
        Assert.Equal("sendgrid-api-key", handler.LastRequest.Headers.Authorization?.Parameter);

        JsonDocument payload = JsonDocument.Parse(handler.LastRequestContent!);
        JsonElement root = payload.RootElement;

        JsonElement listIds = root.GetProperty("list_ids");
        Assert.Equal(JsonValueKind.Array, listIds.ValueKind);
        Assert.Equal("newsletter-list-id", listIds[0].GetString());

        JsonElement contacts = root.GetProperty("contacts");
        Assert.Equal(JsonValueKind.Array, contacts.ValueKind);
        Assert.Equal("person@example.com", contacts[0].GetProperty("email").GetString());
    }

    [Fact]
    public async Task UpsertNewsletterContactAsync_WhenSendGridFails_Throws()
    {
        RecordingHandler handler = new((HttpRequestMessage _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"errors\":[{\"message\":\"bad request\"}]}", Encoding.UTF8, "application/json")
            };
        });

        FakeHttpClientFactory httpClientFactory = new(handler);
        SendGridOptions options = new()
        {
            ApiKey = "sendgrid-api-key",
            NewsletterListId = "newsletter-list-id"
        };

        SendGridNewsletterUpsertService client = new(httpClientFactory, options);

        await Assert.ThrowsAsync<InvalidOperationException>(() => client.UpsertNewsletterContactAsync("person@example.com"));
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient client;

        public FakeHttpClientFactory(HttpMessageHandler handler)
        {
            this.client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.sendgrid.com")
            };
        }

        public HttpClient CreateClient(string name)
        {
            return this.client;
        }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> responseFactory;

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            this.responseFactory = responseFactory;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        public string? LastRequestContent { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.LastRequest = request;
            this.LastRequestContent = request.Content is null
                ? String.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            HttpResponseMessage response = this.responseFactory(request);
            return response;
        }
    }
}
