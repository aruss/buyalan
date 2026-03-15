namespace BuyAlan.WebApi.Onboarding;

using BuyAlan.Onboarding;
using BuyAlan.WebApi.Subscriptions;

public sealed record GetSubscriptionOnboardingStateResult(
    string Status,
    string CurrentStep,
    OnboardingStepState[] Steps,
    Guid? PrimaryAgentId,
    bool CanFinalize,
    OnboardingProfilePrefill ProfilePrefill,
    OnboardingChannelsPrefill ChannelsPrefill,
    OnboardingInvitationStepResult Invitations);
