namespace BuyAlan.SquareIntegration;

public interface ISubscriptionCatalogSyncMessagePublisher
{
    Task PublishAsync(SquareCatalogSyncRequested message, CancellationToken cancellationToken = default);
}
