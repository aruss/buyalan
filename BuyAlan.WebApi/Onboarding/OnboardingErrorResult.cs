namespace BuyAlan.WebApi.Onboarding;

public sealed record OnboardingErrorResult(
    string ErrorCode,
    string Message);
