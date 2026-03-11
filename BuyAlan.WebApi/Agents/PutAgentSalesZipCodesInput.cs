namespace BuyAlan.WebApi.Agents;

public sealed record PutAgentSalesZipCodesInput(
    IReadOnlyCollection<string> ZipCodes);
