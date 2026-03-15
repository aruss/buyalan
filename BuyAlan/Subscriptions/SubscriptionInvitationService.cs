namespace BuyAlan.Subscriptions;

using BuyAlan;
using BuyAlan.Configuration;
using BuyAlan.Data;
using BuyAlan.Data.Entities;
using BuyAlan.Email;
using BuyAlan.Tokens;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed class SubscriptionInvitationService : ISubscriptionInvitationService
{
    private static readonly DateTime IndefiniteExpiryUtc = new(9999, 12, 31, 23, 59, 59, DateTimeKind.Utc);

    private readonly MainDataContext dbContext;
    private readonly ITokenService tokenService;
    private readonly IEmailQueuingService emailQueuingService;
    private readonly AppOptions appOptions;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<SubscriptionInvitationService> logger;

    public SubscriptionInvitationService(
        MainDataContext dbContext,
        ITokenService tokenService,
        IEmailQueuingService emailQueuingService,
        AppOptions appOptions,
        TimeProvider timeProvider,
        ILogger<SubscriptionInvitationService> logger)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        this.tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        this.emailQueuingService = emailQueuingService ?? throw new ArgumentNullException(nameof(emailQueuingService));
        this.appOptions = appOptions ?? throw new ArgumentNullException(nameof(appOptions));
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateSubscriptionInvitationResult> CreateAsync(
        CreateSubscriptionInvitationInput input,
        CancellationToken cancellationToken = default)
    {
        if (!input.Email.TryNormalizeEmail(out string normalizedEmail))
        {
            return new CreateSubscriptionInvitationResult.Failure("email_invalid");
        }

        if (!IsSupportedRole(input.Role))
        {
            return new CreateSubscriptionInvitationResult.Failure("role_invalid");
        }

        bool requesterIsMember = await this.IsSubscriptionMemberAsync(
            input.SubscriptionId,
            input.InvitedByUserId,
            cancellationToken);

        if (!requesterIsMember)
        {
            return new CreateSubscriptionInvitationResult.Failure("subscription_member_required");
        }

        bool alreadyMember = await this.IsEmailAlreadySubscriptionMemberAsync(
            input.SubscriptionId,
            normalizedEmail,
            cancellationToken);

        if (alreadyMember)
        {
            return new CreateSubscriptionInvitationResult.Failure("subscription_user_exists");
        }

        DateTime utcNow = this.timeProvider.GetUtcNow().UtcDateTime;
        SubscriptionInvitation? existingInvitation = await this.GetActiveInvitationAsync(
            input.SubscriptionId,
            normalizedEmail,
            cancellationToken);

        bool wasReusedExistingInvitation = existingInvitation is not null;
        SubscriptionInvitation invitation = existingInvitation ?? new SubscriptionInvitation
        {
            SubscriptionId = input.SubscriptionId,
            Email = normalizedEmail,
            Token = this.tokenService.CreateOpaqueToken()
        };

        invitation.Email = normalizedEmail;
        invitation.Role = input.Role;
        invitation.InvitedByUserId = input.InvitedByUserId;
        invitation.SentAtUtc = utcNow;
        invitation.ExpiresAtUtc = IndefiniteExpiryUtc;

        if (existingInvitation is null)
        {
            this.dbContext.SubscriptionInvitations.Add(invitation);
        }

        await this.dbContext.SaveChangesAsync(cancellationToken);

        SubscriptionInvitationEmailResult result = await this.EnqueueInvitationEmailAsync(invitation, cancellationToken);

        this.logger.LogInformation(
            "Subscription invitation queued. InvitationId={InvitationId} SubscriptionId={SubscriptionId} To={MaskedEmail} Reused={WasReused}",
            invitation.Id,
            invitation.SubscriptionId,
            invitation.Email.RedactEmail(),
            wasReusedExistingInvitation);

        return new CreateSubscriptionInvitationResult.Success(result, wasReusedExistingInvitation);
    }

    public async Task<ResendSubscriptionInvitationResult> ResendAsync(
        Guid invitationId,
        Guid requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        SubscriptionInvitation? invitation = await this.dbContext.SubscriptionInvitations
            .SingleOrDefaultAsync(item => item.Id == invitationId, cancellationToken);

        if (invitation is null)
        {
            return new ResendSubscriptionInvitationResult.Failure("invitation_not_found");
        }

        if (!await this.IsSubscriptionMemberAsync(invitation.SubscriptionId, requestedByUserId, cancellationToken))
        {
            return new ResendSubscriptionInvitationResult.Failure("subscription_member_required");
        }

        string? stateErrorCode = GetInvitationInactiveErrorCode(invitation, this.timeProvider.GetUtcNow().UtcDateTime);
        if (!String.IsNullOrWhiteSpace(stateErrorCode))
        {
            return new ResendSubscriptionInvitationResult.Failure(stateErrorCode);
        }

        invitation.SentAtUtc = this.timeProvider.GetUtcNow().UtcDateTime;
        invitation.ExpiresAtUtc = IndefiniteExpiryUtc;

        await this.dbContext.SaveChangesAsync(cancellationToken);

        SubscriptionInvitationEmailResult result = await this.EnqueueInvitationEmailAsync(invitation, cancellationToken);

        this.logger.LogInformation(
            "Subscription invitation resent. InvitationId={InvitationId} SubscriptionId={SubscriptionId} To={MaskedEmail}",
            invitation.Id,
            invitation.SubscriptionId,
            invitation.Email.RedactEmail());

        return new ResendSubscriptionInvitationResult.Success(result);
    }

    public async Task<CopySubscriptionInvitationLinkResult> CopyLinkAsync(
        Guid invitationId,
        Guid requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        SubscriptionInvitation? invitation = await this.dbContext.SubscriptionInvitations
            .SingleOrDefaultAsync(item => item.Id == invitationId, cancellationToken);

        if (invitation is null)
        {
            return new CopySubscriptionInvitationLinkResult.Failure("invitation_not_found");
        }

        if (!await this.IsSubscriptionMemberAsync(invitation.SubscriptionId, requestedByUserId, cancellationToken))
        {
            return new CopySubscriptionInvitationLinkResult.Failure("subscription_member_required");
        }

        string? stateErrorCode = GetInvitationInactiveErrorCode(invitation, this.timeProvider.GetUtcNow().UtcDateTime);
        if (!String.IsNullOrWhiteSpace(stateErrorCode))
        {
            return new CopySubscriptionInvitationLinkResult.Failure(stateErrorCode);
        }

        return new CopySubscriptionInvitationLinkResult.Success(BuildInvitationUrl(this.appOptions.PublicBaseUrl, invitation.Token));
    }

    public async Task<RevokeSubscriptionInvitationResult> RevokeAsync(
        Guid invitationId,
        Guid requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        SubscriptionInvitation? invitation = await this.dbContext.SubscriptionInvitations
            .SingleOrDefaultAsync(item => item.Id == invitationId, cancellationToken);

        if (invitation is null)
        {
            return new RevokeSubscriptionInvitationResult.Failure("invitation_not_found");
        }

        if (!await this.IsSubscriptionMemberAsync(invitation.SubscriptionId, requestedByUserId, cancellationToken))
        {
            return new RevokeSubscriptionInvitationResult.Failure("subscription_member_required");
        }

        if (invitation.RevokedAtUtc.HasValue)
        {
            return new RevokeSubscriptionInvitationResult.AlreadyRevoked();
        }

        if (invitation.AcceptedAtUtc.HasValue)
        {
            return new RevokeSubscriptionInvitationResult.Failure("invitation_already_accepted");
        }

        invitation.RevokedAtUtc = this.timeProvider.GetUtcNow().UtcDateTime;
        await this.dbContext.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Subscription invitation revoked. InvitationId={InvitationId} SubscriptionId={SubscriptionId} To={MaskedEmail}",
            invitation.Id,
            invitation.SubscriptionId,
            invitation.Email.RedactEmail());

        return new RevokeSubscriptionInvitationResult.Success();
    }

    public async Task<GetSubscriptionInvitationByTokenResult> GetByTokenAsync(
        string? token,
        CancellationToken cancellationToken = default)
    {
        string normalizedToken = token.TrimOrEmpty();
        if (String.IsNullOrWhiteSpace(normalizedToken))
        {
            return new GetSubscriptionInvitationByTokenResult.Failure("invitation_invalid");
        }

        SubscriptionInvitation? invitation = await this.dbContext.SubscriptionInvitations
            .SingleOrDefaultAsync(item => item.Token == normalizedToken, cancellationToken);

        if (invitation is null)
        {
            return new GetSubscriptionInvitationByTokenResult.Failure("invitation_invalid");
        }

        SubscriptionInvitationLookupResult lookupResult = BuildLookupResult(invitation);
        DateTime utcNow = this.timeProvider.GetUtcNow().UtcDateTime;

        if (invitation.RevokedAtUtc.HasValue)
        {
            return new GetSubscriptionInvitationByTokenResult.Revoked(lookupResult);
        }

        if (invitation.AcceptedAtUtc.HasValue)
        {
            return new GetSubscriptionInvitationByTokenResult.Accepted(lookupResult);
        }

        if (invitation.ExpiresAtUtc <= utcNow)
        {
            return new GetSubscriptionInvitationByTokenResult.Expired(lookupResult);
        }

        return new GetSubscriptionInvitationByTokenResult.Success(lookupResult);
    }

    public async Task<AcceptSubscriptionInvitationResult> AcceptAsync(
        AcceptSubscriptionInvitationInput input,
        CancellationToken cancellationToken = default)
    {
        string normalizedToken = input.Token.TrimOrEmpty();
        if (String.IsNullOrWhiteSpace(normalizedToken))
        {
            return new AcceptSubscriptionInvitationResult.Failure("invitation_invalid");
        }

        ApplicationUser? user = await this.dbContext.Users
            .SingleOrDefaultAsync(item => item.Id == input.UserId, cancellationToken);

        if (user is null)
        {
            return new AcceptSubscriptionInvitationResult.Failure("user_not_found");
        }

        if (!user.Email.TryNormalizeEmail(out string normalizedUserEmail))
        {
            return new AcceptSubscriptionInvitationResult.Failure("user_email_invalid");
        }

        SubscriptionInvitation? invitation = await this.dbContext.SubscriptionInvitations
            .SingleOrDefaultAsync(item => item.Token == normalizedToken, cancellationToken);

        if (invitation is null)
        {
            return new AcceptSubscriptionInvitationResult.Failure("invitation_invalid");
        }

        if (!String.Equals(invitation.Email, normalizedUserEmail, StringComparison.OrdinalIgnoreCase))
        {
            return new AcceptSubscriptionInvitationResult.Failure("invitation_email_mismatch");
        }

        if (invitation.RevokedAtUtc.HasValue)
        {
            return new AcceptSubscriptionInvitationResult.Failure("invitation_revoked");
        }

        DateTime utcNow = this.timeProvider.GetUtcNow().UtcDateTime;
        if (invitation.ExpiresAtUtc <= utcNow)
        {
            return new AcceptSubscriptionInvitationResult.Failure("invitation_expired");
        }

        bool membershipExists = await this.dbContext.SubscriptionUsers
            .AnyAsync(
                item => item.SubscriptionId == invitation.SubscriptionId && item.UserId == input.UserId,
                cancellationToken);

        if (invitation.AcceptedAtUtc.HasValue)
        {
            if (!membershipExists)
            {
                return new AcceptSubscriptionInvitationResult.Failure("invitation_already_accepted");
            }

            user.ActiveSubscriptionId = invitation.SubscriptionId;
            await this.dbContext.SaveChangesAsync(cancellationToken);

            this.logger.LogInformation(
                "Previously accepted invitation revisited. InvitationId={InvitationId} SubscriptionId={SubscriptionId} UserId={UserId}",
                invitation.Id,
                invitation.SubscriptionId,
                user.Id);

            return new AcceptSubscriptionInvitationResult.AlreadyAccepted(invitation.SubscriptionId);
        }

        if (!membershipExists)
        {
            SubscriptionUser membership = new()
            {
                SubscriptionId = invitation.SubscriptionId,
                UserId = user.Id,
                Role = invitation.Role
            };

            this.dbContext.SubscriptionUsers.Add(membership);
        }

        invitation.AcceptedAtUtc = utcNow;
        user.ActiveSubscriptionId = invitation.SubscriptionId;

        await this.dbContext.SaveChangesAsync(cancellationToken);

        this.logger.LogInformation(
            "Subscription invitation accepted. InvitationId={InvitationId} SubscriptionId={SubscriptionId} UserId={UserId} MembershipCreated={MembershipCreated}",
            invitation.Id,
            invitation.SubscriptionId,
            user.Id,
            !membershipExists);

        return new AcceptSubscriptionInvitationResult.Success(invitation.SubscriptionId, !membershipExists);
    }

    internal static string BuildInvitationUrl(Uri publicBaseUrl, string token)
    {
        if (String.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token is required.", nameof(token));
        }

        string normalizedBasePath = publicBaseUrl.AbsolutePath.TrimEnd('/');
        PathString basePath = String.Equals(normalizedBasePath, "/", StringComparison.Ordinal) ||
            String.IsNullOrWhiteSpace(normalizedBasePath)
                ? PathString.Empty
                : new PathString(normalizedBasePath);
        PathString invitationPath = basePath.Add("/invite").Add($"/{token.TrimOrEmpty()}");

        UriBuilder uriBuilder = new(publicBaseUrl)
        {
            Path = invitationPath.Value,
            Query = String.Empty,
            Fragment = String.Empty
        };

        return uriBuilder.Uri.ToString();
    }
    private static bool IsSupportedRole(SubscriptionUserRole role)
    {
        return Enum.IsDefined(role) &&
            (role == SubscriptionUserRole.Owner || role == SubscriptionUserRole.Member);
    }
    private static string? GetInvitationInactiveErrorCode(SubscriptionInvitation invitation, DateTime utcNow)
    {
        if (invitation.RevokedAtUtc.HasValue)
        {
            return "invitation_revoked";
        }

        if (invitation.AcceptedAtUtc.HasValue)
        {
            return "invitation_already_accepted";
        }

        if (invitation.ExpiresAtUtc <= utcNow)
        {
            return "invitation_expired";
        }

        return null;
    }

    private static SubscriptionInvitationLookupResult BuildLookupResult(SubscriptionInvitation invitation)
    {
        return new SubscriptionInvitationLookupResult(
            invitation.Id,
            invitation.SubscriptionId,
            invitation.Email.RedactEmail(),
            invitation.Role,
            invitation.SentAtUtc,
            BuildSubscriptionDisplayText(invitation.SubscriptionId));
    }

    private static string BuildSubscriptionDisplayText(Guid subscriptionId)
    {
        return $"Subscription {subscriptionId:D}";
    }

    private async Task<SubscriptionInvitation?> GetActiveInvitationAsync(
        Guid subscriptionId,
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        string normalizedEmailUpper = normalizedEmail.ToUpperInvariant();

        return await this.dbContext.SubscriptionInvitations
            .Where(
                item =>
                    item.SubscriptionId == subscriptionId &&
                    item.AcceptedAtUtc == null &&
                    item.RevokedAtUtc == null &&
                    item.Email.ToUpper() == normalizedEmailUpper)
            .OrderByDescending(item => item.SentAtUtc)
            .ThenByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> IsEmailAlreadySubscriptionMemberAsync(
        Guid subscriptionId,
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        string normalizedEmailUpper = normalizedEmail.ToUpperInvariant();

        return await this.dbContext.SubscriptionUsers
            .AnyAsync(
                item =>
                    item.SubscriptionId == subscriptionId &&
                    item.User.NormalizedEmail == normalizedEmailUpper,
                cancellationToken);
    }

    private async Task<bool> IsSubscriptionMemberAsync(
        Guid subscriptionId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await this.dbContext.SubscriptionUsers
            .AnyAsync(
                item => item.SubscriptionId == subscriptionId && item.UserId == userId,
                cancellationToken);
    }

    private async Task<SubscriptionInvitationEmailResult> EnqueueInvitationEmailAsync(
        SubscriptionInvitation invitation,
        CancellationToken cancellationToken)
    {
        string invitationUrl = BuildInvitationUrl(this.appOptions.PublicBaseUrl, invitation.Token);
        string subscriptionDisplayText = BuildSubscriptionDisplayText(invitation.SubscriptionId);

        EmailSendRequested emailMessage = new(
            invitation.Email,
            EmailTemplateKey.SubscriptionInvitation,
            new Dictionary<string, string>
            {
                ["invitation_url"] = invitationUrl,
                ["subscription_display_text"] = subscriptionDisplayText
            });

        await this.emailQueuingService.EnqueueAsync(emailMessage, cancellationToken);

        return new SubscriptionInvitationEmailResult(
            invitation.Id,
            invitation.SubscriptionId,
            invitation.Email,
            invitation.Role,
            invitation.SentAtUtc,
            invitationUrl,
            subscriptionDisplayText);
    }
}


