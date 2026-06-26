using AssurArr.Api.Data;
using AssurArr.Api.Auth;
using AssurArr.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using STLCompliance.Shared.Auth;
using STLCompliance.Shared.Data;
using STLCompliance.Shared.Hosting;
using STLCompliance.Shared.Integration;
using STLCompliance.Shared.SmartImport;

namespace AssurArr.Api;

public static class AssurArrServiceRegistration
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        if (string.IsNullOrWhiteSpace(StlDatabaseConnection.Resolve(builder.Configuration)))
        {
            builder.Services.AddDbContext<AssurArrDbContext>(options =>
                options.UseInMemoryDatabase("assurarr"));
        }

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddStlNexArrHandoffClient(builder.Configuration);
        builder.Services.Configure<StlServiceTokenOptions>(builder.Configuration.GetSection(StlServiceTokenOptions.SectionName));
        builder.Services.AddSingleton<StlServiceTokenValidator>();
        builder.Services.AddScoped<AssurArrTokenService>();
        builder.Services.AddScoped<AssurArrAuthorizationService>();
        builder.Services.AddScoped<HandoffAuthService>();
        builder.Services.AddScoped<AssurArrQualityService>();
        builder.Services.AddScoped<ISmartImportDestinationCommitHandler, AssurArrSmartImportCommitHandler>();
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(AssurArrAuthorizationPolicies.ProductAccess, policy =>
                policy.RequireAuthenticatedUser());
        });

        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AssurArrTestingAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = AssurArrTestingAuthenticationHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, AssurArrTestingAuthenticationHandler>(
                    AssurArrTestingAuthenticationHandler.SchemeName,
                    _ => { });
        }

        var frontendOrigin = builder.Configuration["Cors:AssurArrFrontendOrigin"] ?? "http://localhost:5183";
        builder.Services.AddStlBrowserCorsPolicy(
            builder.Configuration,
            "AssurArrFrontend",
            frontendOrigin,
            "http://127.0.0.1:5183");
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseCors("AssurArrFrontend");
    }

}
