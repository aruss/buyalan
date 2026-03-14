namespace BuyAlan.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using BuyAlan.Data;
using BuyAlan.Data.Entities;

public class M32GateADataModelTests
{
    [Fact]
    public void MainDataContext_ModelContainsSubscriptionInvitationEntity()
    {
        using MainDataContext context = CreatePostgresContext();

        IEntityType? invitationEntity = context.Model.FindEntityType(typeof(SubscriptionInvitation));

        Assert.NotNull(invitationEntity);
    }

    [Fact]
    public void SubscriptionInvitation_HasExpectedIndexesAndRelationships()
    {
        using MainDataContext context = CreatePostgresContext();
        IEntityType entityType = context.Model.FindEntityType(typeof(SubscriptionInvitation))!;

        IKey primaryKey = entityType.FindPrimaryKey()!;
        Assert.Single(primaryKey.Properties);
        Assert.Equal(nameof(SubscriptionInvitation.Id), primaryKey.Properties[0].Name);

        IIndex? tokenIndex = entityType.GetIndexes()
            .SingleOrDefault(index => index.Properties.Select(property => property.Name).SequenceEqual([nameof(SubscriptionInvitation.Token)]));
        Assert.NotNull(tokenIndex);
        Assert.True(tokenIndex!.IsUnique);

        IForeignKey subscriptionForeignKey = entityType.GetForeignKeys()
            .Single(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(Subscription));
        Assert.Equal(DeleteBehavior.Cascade, subscriptionForeignKey.DeleteBehavior);

        IForeignKey invitedByUserForeignKey = entityType.GetForeignKeys()
            .Single(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(ApplicationUser));
        Assert.Equal(DeleteBehavior.Restrict, invitedByUserForeignKey.DeleteBehavior);
    }

    [Fact]
    public void ApplicationUser_ActiveSubscription_IsOptionalWithSetNullDeleteBehavior()
    {
        using MainDataContext context = CreatePostgresContext();
        IEntityType entityType = context.Model.FindEntityType(typeof(ApplicationUser))!;

        IProperty activeSubscriptionIdProperty = entityType.FindProperty(nameof(ApplicationUser.ActiveSubscriptionId))!;
        Assert.True(activeSubscriptionIdProperty.IsNullable);

        IForeignKey activeSubscriptionForeignKey = entityType.GetForeignKeys()
            .Single(foreignKey => foreignKey.Properties.Select(property => property.Name).SequenceEqual([nameof(ApplicationUser.ActiveSubscriptionId)]));
        Assert.Equal(DeleteBehavior.SetNull, activeSubscriptionForeignKey.DeleteBehavior);
    }

    [Fact]
    public void SubscriptionInvitation_ImplementsAuditAndIdContracts()
    {
        Assert.True(typeof(IEntityWithId).IsAssignableFrom(typeof(SubscriptionInvitation)));
        Assert.True(typeof(IEntityWithAudit).IsAssignableFrom(typeof(SubscriptionInvitation)));
    }

    private static MainDataContext CreatePostgresContext()
    {
        DbContextOptions<MainDataContext> options = new DbContextOptionsBuilder<MainDataContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=buyalan_tests;Username=test;Password=test")
            .Options;

        return new MainDataContext(options);
    }
}
