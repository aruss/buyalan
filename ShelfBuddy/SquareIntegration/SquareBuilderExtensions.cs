namespace ShelfBuddy.SquareIntegration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShelfBuddy.SquareIntegration;

public static class SquareBuilderExtensions
{
    public static TBuilder AddSquareServices<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHttpClient("SquareOAuthClient");
        builder.Services.AddScoped<ISquareTokenService, SquareTokenService>();

        return builder;
    }
}
