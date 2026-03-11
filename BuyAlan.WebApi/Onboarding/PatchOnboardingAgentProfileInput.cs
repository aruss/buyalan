namespace BuyAlan.WebApi.Onboarding;

using BuyAlan.Data.Entities;

public sealed record PatchOnboardingAgentProfileInput(
    string? Name,
    AgentPersonality? Personality);
