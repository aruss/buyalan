namespace SquareBuddy.WebApi.Core;

using Minio;
using SquareBuddy.Configuration;

public static class CoreBuilderExtensions
{
    public static TBuilder AddCoreServices<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        AppOptions options = builder.Configuration.TryGetAppOptions(); 
        builder.Services.AddSingleton(options);

        // ... add here busines services, repositories, etc.

        return builder;
    }
}