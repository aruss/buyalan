namespace HeyAlan.Configuration;

using Microsoft.Extensions.Configuration;

public record MinioOptions
{    
    public Uri? Endpoint { get; init; }
   
    public string AccessKey { get; init; } = String.Empty;
    
    public string SecretKey { get; init; } = String.Empty;

    public string Bucket { get; init; } = String.Empty;
}

public static class MinioConfigurationExtensions
{
    public static MinioOptions TryGetMinioOptions(this IConfiguration configuration)
    {
        var endpointRaw = configuration["MINIO_ENDPOINT"]
            ?? throw ConfigurationErrors.Missing("MINIO_ENDPOINT");

        if (!Uri.TryCreate(endpointRaw, UriKind.Absolute, out var endpoint))
            throw ConfigurationErrors.Invalid("MINIO_ENDPOINT");

        var accessKey = configuration["MINIO_ACCESS_KEY"]
            ?? throw ConfigurationErrors.Missing("MINIO_ACCESS_KEY");

        var secretKey = configuration["MINIO_SECRET_KEY"]
            ?? throw ConfigurationErrors.Missing("MINIO_SECRET_KEY");

        var bucket = configuration["MINIO_BUCKET"]
            ?? throw ConfigurationErrors.Missing("MINIO_BUCKET");

        return new MinioOptions
        {
            Endpoint = endpoint,
            AccessKey = accessKey,
            SecretKey = secretKey,
            Bucket = bucket
        };
    }
}
