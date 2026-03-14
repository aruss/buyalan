namespace BuyAlan.Data.Entities;

using Microsoft.AspNetCore.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>, IEntityWithId, IEntityWithAudit
{
    public string DisplayName { get; set; }

    public Guid? ActiveSubscriptionId { get; set; }

    public Subscription? ActiveSubscription { get; set; }

    public ICollection<SubscriptionUser> SubscriptionUsers { get; set; } = new List<SubscriptionUser>();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
