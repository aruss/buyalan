namespace BuyAlan.SquareIntegration;

using BuyAlan.Data.Entities;

public sealed record SquareCatalogSyncRequested(
    Guid SubscriptionId,
    CatalogSyncTriggerSource TriggerSource,
    bool ForceFullSync = false);
