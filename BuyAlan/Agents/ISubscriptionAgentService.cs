namespace BuyAlan.Agents;

using BuyAlan.Data.Entities;

public interface ISubscriptionAgentService
{
    Task<GetSubscriptionAgentsResult> GetAgentsAsync(
        GetSubscriptionAgentsInput input,
        CancellationToken cancellationToken = default);

    Task<CreateSubscriptionAgentResult> CreateAgentAsync(
        CreateSubscriptionAgentInput input,
        CancellationToken cancellationToken = default);

    Task<GetAgentResult> GetAgentAsync(
        GetAgentInput input,
        CancellationToken cancellationToken = default);

    Task<UpdateAgentResult> UpdateAgentAsync(
        UpdateAgentInput input,
        CancellationToken cancellationToken = default);

    Task<DeleteAgentResult> DeleteAgentAsync(
        DeleteAgentInput input,
        CancellationToken cancellationToken = default);
}

public sealed record GetSubscriptionAgentsInput(
    Guid SubscriptionId,
    Guid UserId);

public sealed record SubscriptionAgentListItem(
    Guid AgentId,
    string Name,
    AgentPersonality? Personality,
    bool IsOperationalReady,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public abstract record GetSubscriptionAgentsResult
{
    public sealed record Success(IReadOnlyList<SubscriptionAgentListItem> Agents) : GetSubscriptionAgentsResult;

    public sealed record Failure(string ErrorCode) : GetSubscriptionAgentsResult;
}

public sealed record CreateSubscriptionAgentInput(
    Guid SubscriptionId,
    Guid UserId);

public abstract record CreateSubscriptionAgentResult
{
    public sealed record Success(AgentDetailsResult Agent) : CreateSubscriptionAgentResult;

    public sealed record Failure(string ErrorCode) : CreateSubscriptionAgentResult;
}

public sealed record GetAgentInput(
    Guid AgentId,
    Guid UserId);

public abstract record GetAgentResult
{
    public sealed record Success(AgentDetailsResult Agent) : GetAgentResult;

    public sealed record Failure(string ErrorCode) : GetAgentResult;
}

public sealed record UpdateAgentInput(
    Guid AgentId,
    Guid UserId,
    string? Name,
    AgentPersonality? Personality,
    string? PersonalityPromptRaw,
    string? TwilioPhoneNumber,
    string? TelegramBotToken,
    string? WhatsappNumber);

public abstract record UpdateAgentResult
{
    public sealed record Success(AgentDetailsResult Agent) : UpdateAgentResult;

    public sealed record Failure(string ErrorCode) : UpdateAgentResult;
}

public sealed record DeleteAgentInput(
    Guid AgentId,
    Guid UserId);

public abstract record DeleteAgentResult
{
    public sealed record Success : DeleteAgentResult;

    public sealed record Failure(string ErrorCode) : DeleteAgentResult;
}

public sealed record AgentDetailsResult(
    Guid AgentId,
    Guid SubscriptionId,
    string Name,
    AgentPersonality? Personality,
    string? PersonalityPromptRaw,
    string? PersonalityPromptSanitized,
    string? TwilioPhoneNumber,
    string? WhatsappNumber,
    string? TelegramBotToken,
    bool IsOperationalReady,
    DateTime CreatedAt,
    DateTime UpdatedAt);
