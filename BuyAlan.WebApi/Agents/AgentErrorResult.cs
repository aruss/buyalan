namespace BuyAlan.WebApi.Agents;

public sealed record AgentErrorResult(
    string ErrorCode,
    string Message);
