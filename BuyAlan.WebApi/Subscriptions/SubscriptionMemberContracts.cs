namespace BuyAlan.WebApi.Subscriptions;

using BuyAlan.Data.Entities;

public sealed record SubscriptionMemberManagementErrorResult(
    string Code,
    string Message);

public sealed record PostSubscriptionInvitationInput(
    string? Email,
    SubscriptionUserRole? Role);

public sealed record PutSubscriptionMemberRoleInput(
    SubscriptionUserRole? Role);

public sealed record GetSubscriptionInvitationLinkResult(
    string InvitationUrl);

public sealed record DeleteSubscriptionInvitationResult(
    bool Revoked);

public sealed record DeleteSubscriptionMemberResult(
    bool Deleted);

public sealed record SubscriptionInvitationItem(
    Guid InvitationId,
    string Email,
    SubscriptionUserRole Role,
    string Status,
    DateTime SentAtUtc,
    DateTime? AcceptedAtUtc,
    DateTime? RevokedAtUtc,
    DateTime ExpiresAtUtc,
    Guid InvitedByUserId,
    string InvitedByDisplayName,
    bool CanResend,
    bool CanCopyLink,
    bool CanRevoke);

public sealed record SubscriptionMemberItem(
    Guid UserId,
    string Email,
    string DisplayName,
    SubscriptionUserRole Role,
    DateTime JoinedAtUtc,
    bool IsCurrentUser,
    bool CanUpdateRole,
    bool CanDelete);

public sealed record GetSubscriptionMembersResult(
    Guid SubscriptionId,
    SubscriptionInvitationItem[] Invitations,
    SubscriptionMemberItem[] Members,
    SubscriptionUserRole[] AvailableRoles);

public sealed record SquareTeamMemberSuggestionItem(
    string DisplayName,
    string Email);

public sealed record OnboardingInvitationStepResult(
    SubscriptionInvitationItem[] Invitations,
    SubscriptionMemberItem[] Members,
    SquareTeamMemberSuggestionItem[] Suggestions,
    SubscriptionUserRole[] AvailableRoles);

public sealed record SubscriptionInvitationLookupItem(
    Guid InvitationId,
    Guid SubscriptionId,
    string MaskedEmail,
    SubscriptionUserRole Role,
    DateTime SentAtUtc,
    string SubscriptionDisplayText);

public sealed record GetSubscriptionInvitationByTokenResult(
    string Status,
    SubscriptionInvitationLookupItem Invitation);

public sealed record PostSubscriptionInvitationAcceptResult(
    string Status,
    Guid SubscriptionId,
    bool MembershipCreated);
