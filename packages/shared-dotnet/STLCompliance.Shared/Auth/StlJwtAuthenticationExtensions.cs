using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace STLCompliance.Shared.Auth;

internal sealed class StlJwtAuthenticationMarker;

public static class StlJwtAuthenticationExtensions
{
    public static IServiceCollection AddStlJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services.Any(d => d.ServiceType == typeof(StlJwtAuthenticationMarker)))
        {
            return services;
        }

        services.AddSingleton<StlJwtAuthenticationMarker>();

        var options = configuration.GetSection(StlJwtOptions.SectionName).Get<StlJwtOptions>() ?? new StlJwtOptions();
        services.Configure<StlJwtOptions>(configuration.GetSection(StlJwtOptions.SectionName));

        var signingKey = configuration["AUTH_SIGNING_KEY"]
            ?? configuration["JWT_SIGNING_KEY"]
            ?? options.SigningKey;
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
        {
            services.AddAuthorization();
            return services;
        }

        var issuer = configuration["JWT_ISSUER"]
            ?? configuration[$"{StlJwtOptions.SectionName}:Issuer"]
            ?? options.Issuer;
        var audience = configuration["JWT_AUDIENCE"]
            ?? configuration[$"{StlJwtOptions.SectionName}:Audience"]
            ?? options.Audience;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.MapInboundClaims = false;
                jwt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Internal routes use service tokens validated in endpoint handlers.
                        if (context.Request.Path.StartsWithSegments("/api/internal", StringComparison.OrdinalIgnoreCase))
                        {
                            context.NoResult();
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Malformed Bearer headers must not surface as unhandled 500s from JWT middleware.
                        context.NoResult();
                        return Task.CompletedTask;
                    }
                };
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "sub"
                };
            });

        services.AddAuthorization();
        return services;
    }
}
