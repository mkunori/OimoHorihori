using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OimoHorihori;
using OimoHorihori.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<SaveService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ServerSaveService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddScoped<ProfileService>();

string? apiBaseUrl = builder.Configuration["ApiBaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl)) { throw new InvalidOperationException("ApiBaseUrl が設定されていません。"); }

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

await builder.Build().RunAsync();
