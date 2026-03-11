namespace BuyAlan.WebApi.Onboarding;

using BuyAlan.Onboarding;

public sealed record GetSubscriptionOnboardingStateResult(
    string Status,
    string CurrentStep,
    OnboardingStepState[] Steps,
    Guid? PrimaryAgentId,
    bool CanFinalize,
    OnboardingProfilePrefill ProfilePrefill,
    OnboardingChannelsPrefill ChannelsPrefill);
