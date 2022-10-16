using Kafe.Wma;
using Marten;
using Microsoft.EntityFrameworkCore;
using Weasel.Core;

namespace Kafe.Migrator;

public static class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((c, s) =>
            {
                s.AddDbContext<WmaContext>(options =>
                {
                    options.UseNpgsql(c.Configuration.GetConnectionString("WMA"));
                }
                );
                s.AddMarten(options =>
                {
                    options.Connection(c.Configuration.GetConnectionString("KAFE"));
                    if (c.HostingEnvironment.IsDevelopment())
                    {
                        options.AutoCreateSchemaObjects = AutoCreate.All;
                    }
                });
            })
            .Build();
        var store = host.Services.GetRequiredService<IDocumentStore>();
    }
}
