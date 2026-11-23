using System;
using System.Threading.Tasks;
using CatalysticBackend.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.NoClobber().Load();
var connectionString =
    $"Host={Environment.GetEnvironmentVariable("PGHOST")};" +
    $"Database={Environment.GetEnvironmentVariable("PGDATABASE")};" +
    $"Username={Environment.GetEnvironmentVariable("PGUSER")};" +
    $"Password={Environment.GetEnvironmentVariable("PGPASSWORD")};" +
    $"SSL Mode={Environment.GetEnvironmentVariable("PGSSLMODE")};" +
    $"Channel Binding={Environment.GetEnvironmentVariable("PGCHANNELBINDING")}";

builder.Services.AddDbContext<CatalysticDbContext>(option =>
{
    option.UseNpgsql(connectionString);
});

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Asistym API",
            Version = "v1"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("../openapi/v1.json", "Asistym API v1");
    });
}
app.UseHttpsRedirection();
app.Run();

