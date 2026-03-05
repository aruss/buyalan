namespace HeyAlan.Newsletter;

public interface INewsletterConfirmationTokenService
{
    string CreateToken(string email, DateTimeOffset issuedAtUtc);

    bool TryReadEmail(string token, out string email);
}
