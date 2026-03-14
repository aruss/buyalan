namespace BuyAlan.Tests;

using System.Net;
using System.Text;
using BuyAlan.Configuration;
using BuyAlan.Data;
using BuyAlan.Onboarding;
using BuyAlan.SquareIntegration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

public class SquareTeamMembersTests
{
    [Fact]
    public async Task GetTeamMembersAsync_WhenSubscriptionHasNoConnection_ReturnsEmptyList()
    {
        MainDataContext dbContext = CreateContext();
        RoutingHandler handler = new(_ => throw new InvalidOperationException("Team API should not be called."));
        SquareService service = CreateService(dbContext, handler);

        IReadOnlyCollection<SquareTeamMemberResult> result = await service.GetTeamMembersAsync(Guid.NewGuid());

        Assert.Empty(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetTeamMembersAsync_WhenTeamMembersReturned_FiltersRowsWithoutEmailAndPaginates()
    {
        MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();

        RoutingHandler handler = new(request =>
        {
            string requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            string payload;
            if (requestBody.Contains("\"cursor\":\"page-2\"", StringComparison.Ordinal))
            {
                payload = """
                {
                  "team_members": [
                    {
                      "given_name": "Zed",
                      "family_name": "Jones",
                      "email_address": "zed@example.com",
                      "status": "ACTIVE"
                    },
                    {
                      "given_name": "No",
                      "family_name": "Email",
                      "status": "ACTIVE"
                    }
                  ]
                }
                """;
            }
            else
            {
                payload = """
                {
                  "team_members": [
                    {
                      "given_name": "Amy",
                      "family_name": "Adams",
                      "email_address": "amy@example.com",
                      "status": "ACTIVE"
                    },
                    {
                      "email_address": "owner@example.com",
                      "status": "ACTIVE"
                    }
                  ],
                  "cursor": "page-2"
                }
                """;
            }

            return JsonResponse(payload);
        });

        SquareService service = CreateService(dbContext, handler);
        await SeedConnectionAsync(service, subscriptionId);

        IReadOnlyCollection<SquareTeamMemberResult> result = await service.GetTeamMembersAsync(subscriptionId);

        SquareTeamMemberResult[] members = result.ToArray();
        Assert.Equal(3, members.Length);
        Assert.Collection(
            members,
            item =>
            {
                Assert.Equal("Amy Adams", item.DisplayName);
                Assert.Equal("amy@example.com", item.Email);
            },
            item =>
            {
                Assert.Equal("owner@example.com", item.DisplayName);
                Assert.Equal("owner@example.com", item.Email);
            },
            item =>
            {
                Assert.Equal("Zed Jones", item.DisplayName);
                Assert.Equal("zed@example.com", item.Email);
            });

        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task GetTeamMembersAsync_WhenTeamApiUnavailable_ReturnsEmptyList()
    {
        MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();

        RoutingHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(
                "{\"errors\":[{\"code\":\"FORBIDDEN\",\"detail\":\"Team API unavailable.\"}]}",
                Encoding.UTF8,
                "application/json")
        });

        SquareService service = CreateService(dbContext, handler);
        await SeedConnectionAsync(service, subscriptionId);

        IReadOnlyCollection<SquareTeamMemberResult> result = await service.GetTeamMembersAsync(subscriptionId);

        Assert.Empty(result);
        Assert.Equal(1, handler.CallCount);
    }

    private static MainDataContext CreateContext()
    {
        DbContextOptions<MainDataContext> options = new DbContextOptionsBuilder<MainDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MainDataContext(options);
    }

    private static SquareService CreateService(MainDataContext dbContext, HttpMessageHandler handler)
    {
        AppOptions appOptions = new()
        {
            PublicBaseUrl = new Uri("https://buyalan.test"),
            SquareClientId = "sandbox-client-id",
            SquareClientSecret = "square-client-secret"
        };

        return new SquareService(
            dbContext,
            new FakeHttpClientFactory(handler),
            appOptions,
            new PassThroughDataProtectionProvider(),
            new PassThroughStateProtector(),
            new StubSubscriptionOnboardingService(),
            new StubSubscriptionCatalogSyncTriggerService(),
            NullLogger<SquareService>.Instance);
    }

    private static Task SeedConnectionAsync(SquareService service, Guid subscriptionId)
    {
        return service.StoreConnectionAsync(new SquareTokenStoreInput(
            subscriptionId,
            Guid.NewGuid(),
            "merchant-1",
            "access-token",
            "refresh-token",
            DateTime.UtcNow.AddMinutes(20),
            ["TEAM_MEMBERS_READ"]));
    }

    private static HttpResponseMessage JsonResponse(string payload)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient client;

        public FakeHttpClientFactory(HttpMessageHandler handler)
        {
            this.client = new HttpClient(handler);
        }

        public HttpClient CreateClient(string name)
        {
            return this.client;
        }
    }

    private sealed class PassThroughDataProtectionProvider : IDataProtectionProvider, IDataProtector
    {
        public IDataProtector CreateProtector(string purpose)
        {
            return this;
        }

        public string Protect(string plaintext)
        {
            return plaintext;
        }

        public string Unprotect(string protectedData)
        {
            return protectedData;
        }

        public byte[] Protect(byte[] plaintext)
        {
            return plaintext;
        }

        public byte[] Unprotect(byte[] protectedData)
        {
            return protectedData;
        }
    }

    private sealed class PassThroughStateProtector : IOAuthStateProtector
    {
        public string Protect(SquareConnectStatePayload payload)
        {
            return "state";
        }

        public bool TryUnprotect(string protectedState, out SquareConnectStatePayload? payload)
        {
            payload = null;
            return false;
        }
    }

    private sealed class StubSubscriptionOnboardingService : ISubscriptionOnboardingService
    {
        public Task<GetSubscriptionOnboardingStateResult> GetStateAsync(Guid subscriptionId, Guid userId, bool resumeMode = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<CreateSubscriptionOnboardingAgentResult> CreatePrimaryAgentAsync(Guid subscriptionId, Guid userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateSubscriptionOnboardingStepResult> UpdateProfileAsync(UpdateSubscriptionOnboardingProfileInput input, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateSubscriptionOnboardingStepResult> UpdateChannelsAsync(UpdateSubscriptionOnboardingChannelsInput input, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateSubscriptionOnboardingStepResult> CompleteInvitationsAsync(Guid subscriptionId, Guid userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateSubscriptionOnboardingStepResult> FinalizeAsync(Guid subscriptionId, Guid userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<UpdateSubscriptionOnboardingStepResult> SkipStepAsync(Guid subscriptionId, Guid userId, string step, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<OnboardingStateResult> RecomputeStateAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new OnboardingStateResult(
                "Draft",
                "square_connect",
                [new OnboardingStepState("square_connect", "in_progress", true, [])],
                null,
                false,
                new OnboardingProfilePrefill(null, null),
                new OnboardingChannelsPrefill(null, null, false)));
        }
    }

    private sealed class RoutingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> responseFactory;

        public RoutingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            this.responseFactory = responseFactory;
        }

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.CallCount++;
            HttpResponseMessage response = this.responseFactory(request);
            return Task.FromResult(response);
        }
    }

    private sealed class StubSubscriptionCatalogSyncTriggerService : ISubscriptionCatalogSyncTriggerService
    {
        public Task<SubscriptionCatalogSyncRequestResult> RequestSyncAsync(
            SubscriptionCatalogSyncRequestInput input,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SubscriptionCatalogSyncRequestResult(true));
        }

        public Task<int> EnqueueDuePeriodicSyncsAsync(
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}