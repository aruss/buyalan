namespace HeyAlan.Newsletter;

public interface ISendGridClient
{
    Task UpsertNewsletterContactAsync(string email, CancellationToken cancellationToken = default);

    Task SendNewsletterConfirmationEmailAsync(
        string email,
        string confirmationUrl,
        CancellationToken cancellationToken = default);
}
