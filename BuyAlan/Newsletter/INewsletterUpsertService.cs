namespace BuyAlan.Newsletter;

public interface INewsletterUpsertService
{
    Task UpsertNewsletterContactAsync(string email, CancellationToken cancellationToken = default);
}
