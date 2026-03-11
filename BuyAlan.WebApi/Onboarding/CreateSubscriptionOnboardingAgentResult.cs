namespace BuyAlan.WebApi.Onboarding;

using BuyAlan.Onboarding;

public sealed record CreateSubscriptionOnboardingAgentResult(
    Guid AgentId,
    GetSubscriptionOnboardingStateResult State);
