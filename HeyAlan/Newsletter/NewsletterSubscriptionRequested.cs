namespace HeyAlan.Newsletter;

public sealed record NewsletterSubscriptionRequested(
    string Email,
    DateTimeOffset RequestedAtUtc);
