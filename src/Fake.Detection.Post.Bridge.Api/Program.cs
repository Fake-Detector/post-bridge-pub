using Fake.Detection.Post.Bridge.Api;
using Fake.Detection.Post.Bridge.Dal.Extensions;
using Microsoft.AspNetCore.Hosting;

var builder = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(x => x.UseStartup<Startup>());

var app = builder.Build();

app.MigrateUp();
app.Run();
