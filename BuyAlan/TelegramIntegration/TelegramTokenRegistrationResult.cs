namespace BuyAlan.TelegramIntegration;

public sealed record TelegramTokenRegistrationResult(
    bool WasAttempted,
    string? ErrorCode);
