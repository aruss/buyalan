namespace HeyAlan.WebApi.Agents;

using HeyAlan.Data.Entities;

public sealed record PostAgentInput(
    string? Name,
    AgentPersonality? Personality,
    string? PersonalityPromptRaw,
    string? TwilioPhoneNumber,
    string? TelegramBotToken,
    string? WhatsappNumber);
