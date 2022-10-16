using Kafe.Lemma;
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
                s.AddDbContext<LemmaContext>(options =>
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
        var lemmaContext = host.Services.GetRequiredService<LemmaContext>();
        var authors = lemmaContext.Authors.OrderBy(a => a.Name).ToList();
        foreach (var author in authors)
        {
            Console.WriteLine(author.Name);
        }
    }
}
