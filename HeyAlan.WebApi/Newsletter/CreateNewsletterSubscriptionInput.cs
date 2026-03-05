namespace HeyAlan.WebApi.Newsletter;

using System.ComponentModel.DataAnnotations;

public sealed record CreateNewsletterSubscriptionInput(
    [Required]
    [EmailAddress]
    string? Email);
