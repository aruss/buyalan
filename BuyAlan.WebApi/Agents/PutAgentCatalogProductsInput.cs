namespace BuyAlan.WebApi.Agents;

public sealed record PutAgentCatalogProductsInput(
    IReadOnlyCollection<Guid> SubscriptionCatalogProductIds);
