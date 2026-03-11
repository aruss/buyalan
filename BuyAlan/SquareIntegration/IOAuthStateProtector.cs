namespace BuyAlan.SquareIntegration;

public sealed record SquareConnectStatePayload(
    Guid SubscriptionId,
    Guid UserId,
    string ReturnUrl,
    DateTime IssuedAtUtc);

public interface IOAuthStateProtector
{
    string Protect(SquareConnectStatePayload payload);

    bool TryUnprotect(string protectedState, out SquareConnectStatePayload? payload);
}
