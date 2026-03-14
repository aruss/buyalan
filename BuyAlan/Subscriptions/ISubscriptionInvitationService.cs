namespace BuyAlan.Subscriptions;

using BuyAlan.Data.Entities;

public sealed record CreateSubscriptionInvitationInput(
    Guid SubscriptionId,
    Guid InvitedByUserId,
    string? Email,
    SubscriptionUserRole Role);

public sealed record AcceptSubscriptionInvitationInput(
    Guid UserId,
    string? Token);

public sealed record SubscriptionInvitationEmailResult(
    Guid InvitationId,
    Guid SubscriptionId,
    string Email,
    SubscriptionUserRole Role,
    DateTime SentAtUtc,
    string InvitationUrl,
    string SubscriptionDisplayText);

public sealed record SubscriptionInvitationLookupResult(
    Guid InvitationId,
    Guid SubscriptionId,
    string MaskedEmail,
    SubscriptionUserRole Role,
    DateTime SentAtUtc,
    string SubscriptionDisplayText);

public abstract record CreateSubscriptionInvitationResult
{
    public sealed record Success(
        SubscriptionInvitationEmailResult Invitation,
        bool WasReusedExistingInvitation)
        : CreateSubscriptionInvitationResult;

    public sealed record Failure(string ErrorCode) : CreateSubscriptionInvitationResult;
}

public abstract record ResendSubscriptionInvitationResult
{
    public sealed record Success(SubscriptionInvitationEmailResult Invitation) : ResendSubscriptionInvitationResult;

    public sealed record Failure(string ErrorCode) : ResendSubscriptionInvitationResult;
}

public abstract record CopySubscriptionInvitationLinkResult
{
    public sealed record Success(string InvitationUrl) : CopySubscriptionInvitationLinkResult;

    public sealed record Failure(string ErrorCode) : CopySubscriptionInvitationLinkResult;
}

public abstract record RevokeSubscriptionInvitationResult
{
    public sealed record Success : RevokeSubscriptionInvitationResult;

    public sealed record AlreadyRevoked : RevokeSubscriptionInvitationResult;

    public sealed record Failure(string ErrorCode) : RevokeSubscriptionInvitationResult;
}

public abstract record GetSubscriptionInvitationByTokenResult
{
    public sealed record Success(SubscriptionInvitationLookupResult Invitation) : GetSubscriptionInvitationByTokenResult;

    public sealed record Accepted(SubscriptionInvitationLookupResult Invitation) : GetSubscriptionInvitationByTokenResult;

    public sealed record Revoked(SubscriptionInvitationLookupResult Invitation) : GetSubscriptionInvitationByTokenResult;

    public sealed record Expired(SubscriptionInvitationLookupResult Invitation) : GetSubscriptionInvitationByTokenResult;

    public sealed record Failure(string ErrorCode) : GetSubscriptionInvitationByTokenResult;
}

public abstract record AcceptSubscriptionInvitationResult
{
    public sealed record Success(Guid SubscriptionId, bool MembershipCreated) : AcceptSubscriptionInvitationResult;

    public sealed record AlreadyAccepted(Guid SubscriptionId) : AcceptSubscriptionInvitationResult;

    public sealed record Failure(string ErrorCode) : AcceptSubscriptionInvitationResult;
}

public interface ISubscriptionInvitationService
{
    Task<CreateSubscriptionInvitationResult> CreateAsync(
        CreateSubscriptionInvitationInput input,
        CancellationToken cancellationToken = default);

    Task<ResendSubscriptionInvitationResult> ResendAsync(
        Guid invitationId,
        Guid requestedByUserId,
        CancellationToken cancellationToken = default);

    Task<CopySubscriptionInvitationLinkResult> CopyLinkAsync(
        Guid invitationId,
        Guid requestedByUserId,
        CancellationToken cancellationToken = default);

    Task<RevokeSubscriptionInvitationResult> RevokeAsync(
        Guid invitationId,
        Guid requestedByUserId,
        CancellationToken cancellationToken = default);

    Task<GetSubscriptionInvitationByTokenResult> GetByTokenAsync(
        string? token,
        CancellationToken cancellationToken = default);

    Task<AcceptSubscriptionInvitationResult> AcceptAsync(
        AcceptSubscriptionInvitationInput input,
        CancellationToken cancellationToken = default);
}
