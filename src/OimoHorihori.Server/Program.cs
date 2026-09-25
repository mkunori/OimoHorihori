using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using OimoHorihori.Server.Data;
using OimoHorihori.Server.Endpoints;
using OimoHorihori.Server.Models;
using OimoHorihori.Server.Security;
using OimoHorihori.Server.Services;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

string[] allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

const string ClientCorsPolicy = "ClientCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        ClientCorsPolicy,
        policy =>
        {
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
                    {
                        return false;
                    }

                    // ローカル開発
                    if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                    {
                        return true;
                    }

                    // 公開Client
                    return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                })
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

string dbPath = builder.Configuration["Database:Path"] ?? Path.Combine(builder.Environment.ContentRootPath, "oimohorihori.db");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddSingleton<AdminAuthService>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCors(ClientCorsPolicy);

app.MapGet("/api/health", () => { return "OK"; });

app.MapAuthEndpoints();
app.MapSaveEndpoints();
app.MapRankingEndpoints();
app.MapProfileEndpoints();
app.MapAccountEndpoints();
app.MapAdminEndpoints();

app.Run();
