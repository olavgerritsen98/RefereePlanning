using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SRO.Api.Auth;
using SRO.Domain.Constraints;
using SRO.Domain.Engine;
using SRO.Domain.Interfaces;
using SRO.Infrastructure.Data;
using SRO.Infrastructure.Services;
using SRO.Infrastructure.Sportlink;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<SroDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SroDatabase"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HeaderTenantProvider>();

// Sportlink HTTP Client with Polly retry
builder.Services.AddHttpClient<ISportlinkClient, SportlinkClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Sportlink:BaseUrl"] ?? "https://data.sportlink.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddStandardResilienceHandler();

// Services
builder.Services.AddScoped<ISyncService, SyncService>();

// Assignment Engine & Constraints
builder.Services.AddScoped<IAssignmentConstraint, TimeConflictConstraint>();
builder.Services.AddScoped<IAssignmentConstraint, LevelConstraint>();
builder.Services.AddScoped<AssignmentEngine>();
builder.Services.AddScoped<IPlanningService, PlanningService>();

// API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck");

app.Run();
