using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using SRO.Domain.Interfaces;
using SRO.Infrastructure.Data;
using SRO.Infrastructure.Services;
using SRO.Infrastructure.Sportlink;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Database
builder.Services.AddDbContext<SroDbContext>(options =>
    options.UseSqlServer(builder.Configuration["ConnectionStrings:SroDatabase"],
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Sportlink HTTP Client
builder.Services.AddHttpClient<ISportlinkClient, SportlinkClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Sportlink:BaseUrl"] ?? "https://data.sportlink.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler();

// Services
builder.Services.AddScoped<ISyncService, SyncService>();

builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults()
    .UseAzureMonitorExporter();

builder.Build().Run();
