using Kafe.Data.Aggregates;
using Marten;
using Marten.Events;
using Weasel.Core;

namespace Kafe.Data;

public static class Db
{
    public static void AddDb(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddMarten(options =>
        {
            options.Connection(configuration.GetConnectionString("KAFE"));
            options.Events.StreamIdentity = StreamIdentity.AsString;
            if (environment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }
            options.CreateDatabasesForTenants(c =>
            {
                c.MaintenanceDatabase(configuration.GetConnectionString("postgres"));
                c.ForTenant()
                    .CheckAgainstPgDatabase()
                    .WithOwner("postgres")
                    .WithEncoding("UTF-8")
                    .ConnectionLimit(-1);
            });

            options.Projections.Add<AuthorProjection>();
        });
    }
}
