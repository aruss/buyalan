namespace BuyAlan.WebApi.Subscriptions;

using BuyAlan.Data;
using BuyAlan.Data.Entities;
using BuyAlan.SquareIntegration;
using Microsoft.EntityFrameworkCore;

internal static class SubscriptionMemberReadModelBuilder
{
    public static async Task<GetSubscriptionMembersResult> BuildMembersResultAsync(
        Guid subscriptionId,
        Guid currentUserId,
        MainDataContext dbContext,
        CancellationToken cancellationToken)
    {
        List<SubscriptionInvitation> invitations = await dbContext.SubscriptionInvitations
            .AsNoTracking()
            .Include(item => item.InvitedByUser)
            .Where(item => item.SubscriptionId == subscriptionId)
            .OrderByDescending(item => item.AcceptedAtUtc == null && item.RevokedAtUtc == null)
            .ThenByDescending(item => item.SentAtUtc)
            .ThenByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        List<SubscriptionUser> members = await dbContext.SubscriptionUsers
            .AsNoTracking()
            .Include(item => item.User)
            .Where(item => item.SubscriptionId == subscriptionId)
            .OrderBy(item => item.Role)
            .ThenBy(item => item.User.DisplayName)
            .ThenBy(item => item.User.Email)
            .ThenBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        int ownerCount = members.Count(item => item.Role == SubscriptionUserRole.Owner);
        DateTime utcNow = DateTime.UtcNow;

        SubscriptionInvitationItem[] invitationItems = invitations
            .Select(item => ToInvitationItem(item, utcNow))
            .ToArray();

        SubscriptionMemberItem[] memberItems = members
            .Select(item => ToMemberItem(item, currentUserId, ownerCount))
            .ToArray();

        return new GetSubscriptionMembersResult(
            subscriptionId,
            invitationItems,
            memberItems,
            [.. Enum.GetValues<SubscriptionUserRole>()]);
    }

    public static async Task<OnboardingInvitationStepResult> BuildOnboardingInvitationStepResultAsync(
        Guid subscriptionId,
        Guid currentUserId,
        MainDataContext dbContext,
        ISquareService squareService,
        CancellationToken cancellationToken)
    {
        GetSubscriptionMembersResult membersResult = await BuildMembersResultAsync(
            subscriptionId,
            currentUserId,
            dbContext,
            cancellationToken);

        HashSet<string> excludedEmails = new(StringComparer.OrdinalIgnoreCase);
        foreach (SubscriptionMemberItem member in membersResult.Members)
        {
            if (!String.IsNullOrWhiteSpace(member.Email))
            {
                excludedEmails.Add(member.Email.Trim());
            }
        }

        foreach (SubscriptionInvitationItem invitation in membersResult.Invitations)
        {
            if (String.Equals(invitation.Status, "pending", StringComparison.Ordinal) &&
                !String.IsNullOrWhiteSpace(invitation.Email))
            {
                excludedEmails.Add(invitation.Email.Trim());
            }
        }

        IReadOnlyCollection<SquareTeamMemberResult> teamMembers = await squareService.GetTeamMembersAsync(
            subscriptionId,
            cancellationToken);

        SquareTeamMemberSuggestionItem[] suggestions = teamMembers
            .Where(item => !String.IsNullOrWhiteSpace(item.Email))
            .GroupBy(item => item.Email.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(group => !excludedEmails.Contains(group.Key))
            .Select(
                group =>
                {
                    SquareTeamMemberResult first = group.First();
                    string displayName = String.IsNullOrWhiteSpace(first.DisplayName)
                        ? first.Email.Trim()
                        : first.DisplayName.Trim();

                    return new SquareTeamMemberSuggestionItem(displayName, first.Email.Trim());
                })
            .OrderBy(item => item.DisplayName)
            .ThenBy(item => item.Email)
            .ToArray();

        return new OnboardingInvitationStepResult(
            membersResult.Invitations,
            membersResult.Members,
            suggestions,
            membersResult.AvailableRoles);
    }

    public static SubscriptionInvitationLookupItem ToLookupItem(
        BuyAlan.Subscriptions.SubscriptionInvitationLookupResult invitation)
    {
        return new SubscriptionInvitationLookupItem(
            invitation.InvitationId,
            invitation.SubscriptionId,
            invitation.MaskedEmail,
            invitation.Role,
            invitation.SentAtUtc,
            invitation.SubscriptionDisplayText);
    }

    private static SubscriptionInvitationItem ToInvitationItem(SubscriptionInvitation invitation, DateTime utcNow)
    {
        string status = ResolveInvitationStatus(invitation, utcNow);
        bool isPending = String.Equals(status, "pending", StringComparison.Ordinal);
        string invitedByDisplayName = String.IsNullOrWhiteSpace(invitation.InvitedByUser.DisplayName)
            ? invitation.InvitedByUser.Email ?? String.Empty
            : invitation.InvitedByUser.DisplayName;

        return new SubscriptionInvitationItem(
            invitation.Id,
            invitation.Email,
            invitation.Role,
            status,
            invitation.SentAtUtc,
            invitation.AcceptedAtUtc,
            invitation.RevokedAtUtc,
            invitation.ExpiresAtUtc,
            invitation.InvitedByUserId,
            invitedByDisplayName,
            isPending,
            isPending,
            isPending);
    }

    private static SubscriptionMemberItem ToMemberItem(
        SubscriptionUser member,
        Guid currentUserId,
        int ownerCount)
    {
        bool isCurrentUser = member.UserId == currentUserId;
        bool isLastOwner = member.Role == SubscriptionUserRole.Owner && ownerCount <= 1;
        string displayName = String.IsNullOrWhiteSpace(member.User.DisplayName)
            ? member.User.Email ?? String.Empty
            : member.User.DisplayName;

        return new SubscriptionMemberItem(
            member.UserId,
            member.User.Email ?? String.Empty,
            displayName,
            member.Role,
            member.CreatedAt,
            isCurrentUser,
            !isLastOwner,
            !isLastOwner);
    }

    private static string ResolveInvitationStatus(SubscriptionInvitation invitation, DateTime utcNow)
    {
        if (invitation.RevokedAtUtc.HasValue)
        {
            return "revoked";
        }

        if (invitation.AcceptedAtUtc.HasValue)
        {
            return "accepted";
        }

        if (invitation.ExpiresAtUtc <= utcNow)
        {
            return "expired";
        }

        return "pending";
    }
}
