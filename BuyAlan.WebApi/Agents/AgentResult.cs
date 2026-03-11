namespace BuyAlan.WebApi.Agents;

using BuyAlan.Data.Entities;

public sealed record AgentResult(
    Guid AgentId,
    Guid SubscriptionId,
    string Name,
    AgentPersonality? Personality,
    string? PersonalityPromptRaw,
    string? TwilioPhoneNumber,
    string? WhatsappNumber,
    string? TelegramBotToken,
    bool IsOperationalReady,
    DateTime CreatedAt,
    DateTime UpdatedAt);
