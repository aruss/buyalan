namespace BuyAlan.WebApi.Newsletter;

using System.ComponentModel.DataAnnotations;

public sealed record CreateNewsletterSubscriptionInput(
    [Required]
    [EmailAddress]
    string? Email);
