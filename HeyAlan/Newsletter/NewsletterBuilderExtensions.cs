namespace HeyAlan.Newsletter;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class NewsletterBuilderExtensions
{
    public static TBuilder AddNewsletterServices<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        SendGridOptions options = builder.Configuration.TryGetSendGridOptions();

        builder.Services.AddSingleton(options);

        builder.Services
            .AddHttpClient("SendGridMarketingClient", client =>
            {
                client.BaseAddress = new Uri("https://api.sendgrid.com");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5)
            });

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<ISendGridClient, SendGridClient>();

        builder.Services.AddSingleton<
            INewsletterConfirmationTokenService,
            NewsletterConfirmationTokenService>();

        return builder;
    }
}
