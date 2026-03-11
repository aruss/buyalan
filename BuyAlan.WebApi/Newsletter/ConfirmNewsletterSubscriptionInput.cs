namespace BuyAlan.WebApi.Newsletter;

using System.ComponentModel.DataAnnotations;

public sealed record ConfirmNewsletterSubscriptionInput(
    [Required]
    string? Token);
