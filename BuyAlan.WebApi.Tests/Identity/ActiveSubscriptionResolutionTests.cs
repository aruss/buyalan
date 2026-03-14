namespace BuyAlan.WebApi.Tests;

using BuyAlan.Data;
using BuyAlan.Data.Entities;
using BuyAlan.WebApi.Identity;
using Microsoft.EntityFrameworkCore;

public class ActiveSubscriptionResolutionTests
{
    [Fact]
    public async Task GetActiveSubscriptionIdAsync_PrefersPersistedActiveSubscriptionWhenMembershipStillExists()
    {
        using MainDataContext context = CreateContext();
        Guid userId = Guid.NewGuid();
        Guid ownerSubscriptionId = Guid.NewGuid();
        Guid memberSubscriptionId = Guid.NewGuid();

        SeedUserAndSubscription(context, userId, ownerSubscriptionId, SubscriptionUserRole.Owner);
        SeedUserAndSubscription(context, userId, memberSubscriptionId, SubscriptionUserRole.Member);
        await context.SaveChangesAsync();

        ApplicationUser user = await context.Users.SingleAsync(item => item.Id == userId);
        user.ActiveSubscriptionId = memberSubscriptionId;
        await context.SaveChangesAsync();

        Guid? activeSubscriptionId = await IdentityEndpoints.GetActiveSubscriptionIdAsync(userId, context);

        Assert.Equal(memberSubscriptionId, activeSubscriptionId);
    }

    [Fact]
    public async Task GetActiveSubscriptionIdAsync_FallsBackWhenPersistedActiveSubscriptionIsMissingFromMemberships()
    {
        using MainDataContext context = CreateContext();
        Guid userId = Guid.NewGuid();
        Guid ownerSubscriptionId = Guid.NewGuid();
        Guid staleSubscriptionId = Guid.NewGuid();

        SeedUserAndSubscription(context, userId, ownerSubscriptionId, SubscriptionUserRole.Owner);
        await context.SaveChangesAsync();

        ApplicationUser user = await context.Users.SingleAsync(item => item.Id == userId);
        user.ActiveSubscriptionId = staleSubscriptionId;
        await context.SaveChangesAsync();

        Guid? activeSubscriptionId = await IdentityEndpoints.GetActiveSubscriptionIdAsync(userId, context);

        Assert.Equal(ownerSubscriptionId, activeSubscriptionId);
    }

    private static MainDataContext CreateContext()
    {
        DbContextOptions<MainDataContext> options = new DbContextOptionsBuilder<MainDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MainDataContext(options);
    }

    private static void SeedUserAndSubscription(
        MainDataContext dbContext,
        Guid userId,
        Guid subscriptionId,
        SubscriptionUserRole role)
    {
        ApplicationUser? trackedUser = dbContext.Users.Local.SingleOrDefault(user => user.Id == userId);
        if (trackedUser is null)
        {
            trackedUser = new ApplicationUser
            {
                Id = userId,
                DisplayName = "Gate A Test User",
                UserName = "gate-a-test@example.com",
                Email = "gate-a-test@example.com"
            };

            dbContext.Users.Add(trackedUser);
        }

        dbContext.Subscriptions.Add(new Subscription
        {
            Id = subscriptionId
        });

        dbContext.SubscriptionUsers.Add(new SubscriptionUser
        {
            SubscriptionId = subscriptionId,
            UserId = userId,
            User = trackedUser,
            Role = role
        });
    }
}
