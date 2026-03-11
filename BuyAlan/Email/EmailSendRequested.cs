namespace BuyAlan.Email;

public sealed record EmailSendRequested(
    string RecipientEmail,
    string TemplateKey,
    Dictionary<string, string> TemplateData);
