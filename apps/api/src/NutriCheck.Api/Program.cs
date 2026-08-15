using Microsoft.EntityFrameworkCore;
using NutriCheck.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Missing required configuration 'ConnectionStrings:Default' (Postgres connection string).");
}

builder.Services.AddDbContext<NutriCheckDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck");

app.Run();
