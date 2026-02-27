namespace SquareBuddy.Shared;

public interface ITelegramService
{
    Task RegisterWebhookAsync(string botToken, CancellationToken ct = default);
}
