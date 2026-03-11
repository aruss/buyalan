namespace BuyAlan.WebApi.Infrastructure;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using BuyAlan.Data;

public static class EntityFrameworkBuilderExtensions
{
    public static TBuilder AddDatabaseServices<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddDbContext<MainDataContext>(options =>
        {
            var connStr = builder.Configuration.GetConnectionString("buyalan");
            if (String.IsNullOrWhiteSpace(connStr))
            {
                throw new InvalidOperationException("DB Connection Missing");
            }

            options.UseNpgsql(connStr);
        });

        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<MainDataContext>();


        return builder; 
    }
}
