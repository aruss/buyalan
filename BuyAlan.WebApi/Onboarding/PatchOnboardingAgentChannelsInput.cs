namespace BuyAlan.WebApi.Onboarding;

public sealed record PatchOnboardingAgentChannelsInput(
    string? TwilioPhoneNumber,
    string? TelegramBotToken,
    string? WhatsappNumber);
