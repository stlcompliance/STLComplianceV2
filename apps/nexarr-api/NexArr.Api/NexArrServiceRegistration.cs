using Microsoft.EntityFrameworkCore;
using NexArr.Api.Data;
using NexArr.Api.Services;
using STLCompliance.Shared.Auth;

namespace NexArr.Api;

public static class NexArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddStlJwtAuthentication(builder.Configuration);
        builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IPlatformAuditService, PlatformAuditService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<PlatformAuthorizationService>();
        builder.Services.AddScoped<TenantAdminService>();
        builder.Services.AddScoped<ProductCatalogService>();
        builder.Services.AddScoped<EntitlementAdminService>();
        builder.Services.AddScoped<ServiceTokenAdminService>();
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
    }

    public static async Task InitializeAsync(WebApplication app)
    {
        if (string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("Database"))
            && string.IsNullOrWhiteSpace(app.Configuration["DATABASE_URL"]))
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<NexArrDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
        {
            await PlatformSeeder.SeedAsync(db, passwordHasher);
        }
    }
}
