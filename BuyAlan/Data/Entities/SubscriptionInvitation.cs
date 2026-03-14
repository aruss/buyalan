namespace BuyAlan.Data.Entities;

public sealed class SubscriptionInvitation : IEntityWithId, IEntityWithAudit
{
    public Guid Id { get; set; }

    public Guid SubscriptionId { get; set; }

    public Subscription Subscription { get; set; } = null!;

    public string Email { get; set; } = String.Empty;

    public SubscriptionUserRole Role { get; set; } = SubscriptionUserRole.Member;

    public string Token { get; set; } = String.Empty;

    public Guid InvitedByUserId { get; set; }

    public ApplicationUser InvitedByUser { get; set; } = null!;

    public DateTime SentAtUtc { get; set; }

    public DateTime? AcceptedAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
