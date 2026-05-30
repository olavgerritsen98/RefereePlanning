using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SRO.Web;
using SRO.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient<MatchApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5299/");
    client.DefaultRequestHeaders.Add("X-Tenant-Id", builder.Configuration["TenantId"] ?? "d34b229f-5549-496a-89da-dcea458f669f");
});

await builder.Build().RunAsync();
