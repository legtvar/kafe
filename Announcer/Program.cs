using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseRewriter(new RewriteOptions()
    .AddRewrite("^.*$", $"/{builder.Configuration.GetValue<string>("Annoucement")}", true));
app.UseStaticFiles();

app.Run();
