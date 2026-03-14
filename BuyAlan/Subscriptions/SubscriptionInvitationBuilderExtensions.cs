namespace BuyAlan.Subscriptions;

using BuyAlan.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class SubscriptionInvitationBuilderExtensions
{
    public static TBuilder AddSubscriptionInvitationServices<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddSingleton<ITokenService, OpaqueTokenService>();
        builder.Services.AddScoped<ISubscriptionInvitationService, SubscriptionInvitationService>();

        return builder;
    }
}
