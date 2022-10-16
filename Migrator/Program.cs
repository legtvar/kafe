using Marten;
using Weasel.Core;

namespace Kafe.Migrator;

public static class Program
{
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((c, s) => s.AddMarten(options => {
                options.Connection(c.Configuration.GetConnectionString("Marten"));
                if (c.HostingEnvironment.IsDevelopment())
                {
                    options.AutoCreateSchemaObjects = AutoCreate.All;
                }
            }))
            .Build();
        var store = host.Services.GetRequiredService<IDocumentStore>();
    }
}
