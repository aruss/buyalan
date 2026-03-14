namespace BuyAlan.Tests;

using BuyAlan.Configuration;
using BuyAlan.Data;
using BuyAlan.Data.Entities;
using BuyAlan.Email;
using BuyAlan.Subscriptions;
using BuyAlan.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class SubscriptionInvitationServiceTests
{
    private static readonly DateTime IndefiniteExpiryUtc = new(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    [Fact]
    public async Task CreateAsync_WhenInvitationIsNew_PersistsInvitationAndQueuesEmail()
    {
        using MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();
        Guid inviterUserId = Guid.NewGuid();
        SeedSubscriptionMember(dbContext, subscriptionId, inviterUserId, "owner@example.com", SubscriptionUserRole.Owner);

        RecordingEmailQueuingService emailQueuingService = new();
        RecordingTokenService tokenService = new("token-1");
        RecordingLogger<SubscriptionInvitationService> logger = new();
        FixedTimeProvider timeProvider = new(DateTimeOffset.Parse("2026-03-13T10:00:00Z"));
        SubscriptionInvitationService service = CreateService(dbContext, emailQueuingService, tokenService, logger, timeProvider);

        CreateSubscriptionInvitationResult result = await service.CreateAsync(new CreateSubscriptionInvitationInput(
            subscriptionId,
            inviterUserId,
            " person@example.com ",
            SubscriptionUserRole.Member));

        CreateSubscriptionInvitationResult.Success success = Assert.IsType<CreateSubscriptionInvitationResult.Success>(result);
        SubscriptionInvitation invitation = Assert.Single(dbContext.SubscriptionInvitations);

        Assert.False(success.WasReusedExistingInvitation);
        Assert.Equal("person@example.com", invitation.Email);
        Assert.Equal(SubscriptionUserRole.Member, invitation.Role);
        Assert.Equal("token-1", invitation.Token);
        Assert.Equal(new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc), invitation.ExpiresAtUtc);
        Assert.Equal(EmailTemplateKey.SubscriptionInvitation, emailQueuingService.LastMessage!.TemplateKey);
        Assert.Equal("https://buyalan.test/tenant/invite/token-1", emailQueuingService.LastMessage.TemplateData["invitation_url"]);
        Assert.Equal($"Subscription {subscriptionId:D}", emailQueuingService.LastMessage.TemplateData["subscription_display_text"]);
        Assert.All(logger.Messages, message => Assert.DoesNotContain("token-1", message, StringComparison.Ordinal));
        Assert.All(logger.Messages, message => Assert.DoesNotContain("https://buyalan.test/tenant/invite/token-1", message, StringComparison.Ordinal));
        Assert.All(logger.Messages, message => Assert.DoesNotContain("person@example.com", message, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(logger.Messages, message => message.Contains("p***@example.com", StringComparison.Ordinal));
    }

    [Fact]
    public async Task CreateAsync_WhenActiveInvitationExists_ReusesInvitationAndPreservesToken()
    {
        using MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();
        Guid inviterUserId = Guid.NewGuid();
        Guid secondInviterUserId = Guid.NewGuid();
        SeedSubscriptionMember(dbContext, subscriptionId, inviterUserId, "owner@example.com", SubscriptionUserRole.Owner);
        SeedSubscriptionMember(dbContext, subscriptionId, secondInviterUserId, "member@example.com", SubscriptionUserRole.Member);

        SubscriptionInvitation existingInvitation = new()
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscriptionId,
            Email = "person@example.com",
            Role = SubscriptionUserRole.Member,
            Token = "existing-token",
            InvitedByUserId = inviterUserId,
            SentAtUtc = DateTime.Parse("2026-03-10T09:00:00Z").ToUniversalTime(),
            ExpiresAtUtc = IndefiniteExpiryUtc
        };

        dbContext.SubscriptionInvitations.Add(existingInvitation);
        await dbContext.SaveChangesAsync();

        RecordingEmailQueuingService emailQueuingService = new();
        RecordingTokenService tokenService = new("unused-token");
        SubscriptionInvitationService service = CreateService(
            dbContext,
            emailQueuingService,
            tokenService,
            NullLogger<SubscriptionInvitationService>.Instance,
            new FixedTimeProvider(DateTimeOffset.Parse("2026-03-13T12:00:00Z")));

        CreateSubscriptionInvitationResult result = await service.CreateAsync(new CreateSubscriptionInvitationInput(
            subscriptionId,
            secondInviterUserId,
            "PERSON@example.com",
            SubscriptionUserRole.Owner));

        CreateSubscriptionInvitationResult.Success success = Assert.IsType<CreateSubscriptionInvitationResult.Success>(result);
        SubscriptionInvitation invitation = Assert.Single(dbContext.SubscriptionInvitations);

        Assert.True(success.WasReusedExistingInvitation);
        Assert.Equal(existingInvitation.Id, invitation.Id);
        Assert.Equal("existing-token", invitation.Token);
        Assert.Equal(secondInviterUserId, invitation.InvitedByUserId);
        Assert.Equal(SubscriptionUserRole.Owner, invitation.Role);
        Assert.Equal("https://buyalan.test/tenant/invite/existing-token", emailQueuingService.LastMessage!.TemplateData["invitation_url"]);
        Assert.Equal(0, tokenService.CallCount);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyMember_ReturnsFailure()
    {
        using MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();
        Guid inviterUserId = Guid.NewGuid();
        SeedSubscriptionMember(dbContext, subscriptionId, inviterUserId, "owner@example.com", SubscriptionUserRole.Owner);
        SeedSubscriptionMember(dbContext, subscriptionId, Guid.NewGuid(), "person@example.com", SubscriptionUserRole.Member);

        SubscriptionInvitationService service = CreateService(dbContext);

        CreateSubscriptionInvitationResult result = await service.CreateAsync(new CreateSubscriptionInvitationInput(
            subscriptionId,
            inviterUserId,
            "person@example.com",
            SubscriptionUserRole.Member));

        CreateSubscriptionInvitationResult.Failure failure = Assert.IsType<CreateSubscriptionInvitationResult.Failure>(result);
        Assert.Equal("subscription_user_exists", failure.ErrorCode);
        Assert.Empty(dbContext.SubscriptionInvitations);
    }

    [Fact]
    public async Task ResendCopyLinkAndRevokeAsync_UseStoredTokenAndTrackState()
    {
        using MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();
        Guid requesterUserId = Guid.NewGuid();
        SeedSubscriptionMember(dbContext, subscriptionId, requesterUserId, "owner@example.com", SubscriptionUserRole.Owner);

        SubscriptionInvitation invitation = new()
        {
            SubscriptionId = subscriptionId,
            Email = "person@example.com",
            Role = SubscriptionUserRole.Member,
            Token = "stored-token",
            InvitedByUserId = requesterUserId,
            SentAtUtc = DateTime.Parse("2026-03-10T09:00:00Z").ToUniversalTime(),
            ExpiresAtUtc = IndefiniteExpiryUtc
        };

        dbContext.SubscriptionInvitations.Add(invitation);
        await dbContext.SaveChangesAsync();

        RecordingEmailQueuingService emailQueuingService = new();
        SubscriptionInvitationService service = CreateService(
            dbContext,
            emailQueuingService,
            new RecordingTokenService("unused-token"),
            NullLogger<SubscriptionInvitationService>.Instance,
            new FixedTimeProvider(DateTimeOffset.Parse("2026-03-13T14:00:00Z")));

        ResendSubscriptionInvitationResult resendResult = await service.ResendAsync(invitation.Id, requesterUserId);
        CopySubscriptionInvitationLinkResult copyLinkResult = await service.CopyLinkAsync(invitation.Id, requesterUserId);
        RevokeSubscriptionInvitationResult revokeResult = await service.RevokeAsync(invitation.Id, requesterUserId);

        Assert.IsType<ResendSubscriptionInvitationResult.Success>(resendResult);
        CopySubscriptionInvitationLinkResult.Success copyLinkSuccess = Assert.IsType<CopySubscriptionInvitationLinkResult.Success>(copyLinkResult);
        Assert.Equal("https://buyalan.test/tenant/invite/stored-token", copyLinkSuccess.InvitationUrl);
        Assert.IsType<RevokeSubscriptionInvitationResult.Success>(revokeResult);
        Assert.Equal(EmailTemplateKey.SubscriptionInvitation, emailQueuingService.LastMessage!.TemplateKey);
        Assert.NotNull(invitation.RevokedAtUtc);
    }

    [Fact]
    public async Task GetByTokenAsync_ReturnsDeterministicStates()
    {
        using MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();
        Guid inviterUserId = Guid.NewGuid();
        SeedSubscriptionMember(dbContext, subscriptionId, inviterUserId, "owner@example.com", SubscriptionUserRole.Owner);

        SubscriptionInvitation activeInvitation = new()
        {
            SubscriptionId = subscriptionId,
            Email = "active@example.com",
            Role = SubscriptionUserRole.Member,
            Token = "active-token",
            InvitedByUserId = inviterUserId,
            SentAtUtc = DateTime.Parse("2026-03-10T09:00:00Z").ToUniversalTime(),
            ExpiresAtUtc = IndefiniteExpiryUtc
        };

        SubscriptionInvitation acceptedInvitation = new()
        {
            SubscriptionId = subscriptionId,
            Email = "accepted@example.com",
            Role = SubscriptionUserRole.Member,
            Token = "accepted-token",
            InvitedByUserId = inviterUserId,
            SentAtUtc = DateTime.Parse("2026-03-10T09:00:00Z").ToUniversalTime(),
            AcceptedAtUtc = DateTime.Parse("2026-03-11T09:00:00Z").ToUniversalTime(),
            ExpiresAtUtc = IndefiniteExpiryUtc
        };

        SubscriptionInvitation revokedInvitation = new()
        {
            SubscriptionId = subscriptionId,
            Email = "revoked@example.com",
            Role = SubscriptionUserRole.Member,
            Token = "revoked-token",
            InvitedByUserId = inviterUserId,
            SentAtUtc = DateTime.Parse("2026-03-10T09:00:00Z").ToUniversalTime(),
            RevokedAtUtc = DateTime.Parse("2026-03-11T09:00:00Z").ToUniversalTime(),
            ExpiresAtUtc = IndefiniteExpiryUtc
        };

        SubscriptionInvitation expiredInvitation = new()
        {
            SubscriptionId = subscriptionId,
            Email = "expired@example.com",
            Role = SubscriptionUserRole.Member,
            Token = "expired-token",
            InvitedByUserId = inviterUserId,
            SentAtUtc = DateTime.Parse("2026-03-10T09:00:00Z").ToUniversalTime(),
            ExpiresAtUtc = DateTime.Parse("2026-03-12T23:59:59Z").ToUniversalTime()
        };

        dbContext.SubscriptionInvitations.AddRange(activeInvitation, acceptedInvitation, revokedInvitation, expiredInvitation);
        await dbContext.SaveChangesAsync();

        SubscriptionInvitationService service = CreateService(
            dbContext,
            timeProvider: new FixedTimeProvider(DateTimeOffset.Parse("2026-03-13T15:00:00Z")));

        Assert.IsType<GetSubscriptionInvitationByTokenResult.Success>(await service.GetByTokenAsync("active-token"));
        Assert.IsType<GetSubscriptionInvitationByTokenResult.Accepted>(await service.GetByTokenAsync("accepted-token"));
        Assert.IsType<GetSubscriptionInvitationByTokenResult.Revoked>(await service.GetByTokenAsync("revoked-token"));
        Assert.IsType<GetSubscriptionInvitationByTokenResult.Expired>(await service.GetByTokenAsync("expired-token"));

        GetSubscriptionInvitationByTokenResult.Failure invalid = Assert.IsType<GetSubscriptionInvitationByTokenResult.Failure>(
            await service.GetByTokenAsync("missing-token"));

        Assert.Equal("invitation_invalid", invalid.ErrorCode);
    }

    [Fact]
    public async Task AcceptAsync_WhenEmailMatches_CreatesMembershipAndSwitchesActiveSubscription()
    {
        using MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();
        Guid inviterUserId = Guid.NewGuid();
        Guid invitedUserId = Guid.NewGuid();
        SeedSubscriptionMember(dbContext, subscriptionId, inviterUserId, "owner@example.com", SubscriptionUserRole.Owner);

        ApplicationUser invitedUser = new()
        {
            Id = invitedUserId,
            Email = "person@example.com",
            NormalizedEmail = "PERSON@EXAMPLE.COM",
            UserName = "person@example.com",
            DisplayName = "Person"
        };

        SubscriptionInvitation invitation = new()
        {
            SubscriptionId = subscriptionId,
            Email = "person@example.com",
            Role = SubscriptionUserRole.Member,
            Token = "accept-token",
            InvitedByUserId = inviterUserId,
            SentAtUtc = DateTime.Parse("2026-03-10T09:00:00Z").ToUniversalTime(),
            ExpiresAtUtc = IndefiniteExpiryUtc
        };

        dbContext.Users.Add(invitedUser);
        dbContext.SubscriptionInvitations.Add(invitation);
        await dbContext.SaveChangesAsync();

        SubscriptionInvitationService service = CreateService(
            dbContext,
            timeProvider: new FixedTimeProvider(DateTimeOffset.Parse("2026-03-13T16:00:00Z")));

        AcceptSubscriptionInvitationResult result = await service.AcceptAsync(new AcceptSubscriptionInvitationInput(invitedUserId, "accept-token"));

        AcceptSubscriptionInvitationResult.Success success = Assert.IsType<AcceptSubscriptionInvitationResult.Success>(result);
        SubscriptionUser membership = Assert.Single(dbContext.SubscriptionUsers.Where(item => item.UserId == invitedUserId));

        Assert.True(success.MembershipCreated);
        Assert.Equal(subscriptionId, success.SubscriptionId);
        Assert.Equal(subscriptionId, invitedUser.ActiveSubscriptionId);
        Assert.Equal(SubscriptionUserRole.Member, membership.Role);
        Assert.NotNull(invitation.AcceptedAtUtc);
    }

    [Fact]
    public async Task AcceptAsync_WhenAlreadyAccepted_SetsActiveSubscriptionAndReturnsAlreadyAccepted()
    {
        using MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();
        Guid inviterUserId = Guid.NewGuid();
        Guid invitedUserId = Guid.NewGuid();
        SeedSubscriptionMember(dbContext, subscriptionId, inviterUserId, "owner@example.com", SubscriptionUserRole.Owner);

        ApplicationUser invitedUser = new()
        {
            Id = invitedUserId,
            Email = "person@example.com",
            NormalizedEmail = "PERSON@EXAMPLE.COM",
            UserName = "person@example.com",
            DisplayName = "Person"
        };

        SubscriptionInvitation invitation = new()
        {
            SubscriptionId = subscriptionId,
            Email = "person@example.com",
            Role = SubscriptionUserRole.Member,
            Token = "accepted-token",
            InvitedByUserId = inviterUserId,
            SentAtUtc = DateTime.Parse("2026-03-10T09:00:00Z").ToUniversalTime(),
            AcceptedAtUtc = DateTime.Parse("2026-03-11T09:00:00Z").ToUniversalTime(),
            ExpiresAtUtc = IndefiniteExpiryUtc
        };

        dbContext.Users.Add(invitedUser);
        dbContext.SubscriptionUsers.Add(new SubscriptionUser
        {
            SubscriptionId = subscriptionId,
            UserId = invitedUserId,
            Role = SubscriptionUserRole.Member
        });
        dbContext.SubscriptionInvitations.Add(invitation);
        await dbContext.SaveChangesAsync();

        SubscriptionInvitationService service = CreateService(dbContext);

        AcceptSubscriptionInvitationResult result = await service.AcceptAsync(new AcceptSubscriptionInvitationInput(invitedUserId, "accepted-token"));

        AcceptSubscriptionInvitationResult.AlreadyAccepted alreadyAccepted = Assert.IsType<AcceptSubscriptionInvitationResult.AlreadyAccepted>(result);
        Assert.Equal(subscriptionId, alreadyAccepted.SubscriptionId);
        Assert.Equal(subscriptionId, invitedUser.ActiveSubscriptionId);
    }

    [Fact]
    public async Task AcceptAsync_WhenEmailMismatch_ReturnsFailure()
    {
        using MainDataContext dbContext = CreateContext();
        Guid subscriptionId = Guid.NewGuid();
        Guid inviterUserId = Guid.NewGuid();
        Guid invitedUserId = Guid.NewGuid();
        SeedSubscriptionMember(dbContext, subscriptionId, inviterUserId, "owner@example.com", SubscriptionUserRole.Owner);

        ApplicationUser invitedUser = new()
        {
            Id = invitedUserId,
            Email = "other@example.com",
            NormalizedEmail = "OTHER@EXAMPLE.COM",
            UserName = "other@example.com",
            DisplayName = "Other"
        };

        SubscriptionInvitation invitation = new()
        {
            SubscriptionId = subscriptionId,
            Email = "person@example.com",
            Role = SubscriptionUserRole.Member,
            Token = "mismatch-token",
            InvitedByUserId = inviterUserId,
            SentAtUtc = DateTime.Parse("2026-03-10T09:00:00Z").ToUniversalTime(),
            ExpiresAtUtc = IndefiniteExpiryUtc
        };

        dbContext.Users.Add(invitedUser);
        dbContext.SubscriptionInvitations.Add(invitation);
        await dbContext.SaveChangesAsync();

        SubscriptionInvitationService service = CreateService(dbContext);

        AcceptSubscriptionInvitationResult result = await service.AcceptAsync(new AcceptSubscriptionInvitationInput(invitedUserId, "mismatch-token"));

        AcceptSubscriptionInvitationResult.Failure failure = Assert.IsType<AcceptSubscriptionInvitationResult.Failure>(result);
        Assert.Equal("invitation_email_mismatch", failure.ErrorCode);
        Assert.Null(invitedUser.ActiveSubscriptionId);
        Assert.Empty(dbContext.SubscriptionUsers.Where(item => item.UserId == invitedUserId));
    }

    private static SubscriptionInvitationService CreateService(
        MainDataContext dbContext,
        RecordingEmailQueuingService? emailQueuingService = null,
        RecordingTokenService? tokenService = null,
        ILogger<SubscriptionInvitationService>? logger = null,
        TimeProvider? timeProvider = null)
    {
        return new SubscriptionInvitationService(
            dbContext,
            tokenService ?? new RecordingTokenService("generated-token"),
            emailQueuingService ?? new RecordingEmailQueuingService(),
            new AppOptions
            {
                PublicBaseUrl = new Uri("https://buyalan.test/tenant")
            },
            timeProvider ?? new FixedTimeProvider(DateTimeOffset.Parse("2026-03-13T10:00:00Z")),
            logger ?? NullLogger<SubscriptionInvitationService>.Instance);
    }

    private static MainDataContext CreateContext()
    {
        DbContextOptions<MainDataContext> options = new DbContextOptionsBuilder<MainDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MainDataContext(options);
    }

    private static void SeedSubscriptionMember(
        MainDataContext dbContext,
        Guid subscriptionId,
        Guid userId,
        string email,
        SubscriptionUserRole role)
    {
        Subscription? existingSubscription = dbContext.Subscriptions.SingleOrDefault(item => item.Id == subscriptionId);
        if (existingSubscription is null)
        {
            existingSubscription = new Subscription
            {
                Id = subscriptionId,
                SubscriptionCreditBalance = 0,
                TopUpCreditBalance = 0
            };

            dbContext.Subscriptions.Add(existingSubscription);
        }

        ApplicationUser user = new()
        {
            Id = userId,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = email,
            DisplayName = email
        };

        dbContext.Users.Add(user);
        dbContext.SubscriptionUsers.Add(new SubscriptionUser
        {
            SubscriptionId = subscriptionId,
            UserId = userId,
            Role = role
        });
        dbContext.SaveChanges();
    }

    private sealed class RecordingEmailQueuingService : IEmailQueuingService
    {
        public EmailSendRequested? LastMessage { get; private set; }

        public Task EnqueueAsync(EmailSendRequested message, CancellationToken cancellationToken = default)
        {
            this.LastMessage = message;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingTokenService : ITokenService
    {
        private readonly string token;

        public RecordingTokenService(string token)
        {
            this.token = token;
        }

        public int CallCount { get; private set; }

        public string CreateOpaqueToken()
        {
            this.CallCount++;
            return this.token;
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            this.utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return this.utcNow;
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            this.Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
