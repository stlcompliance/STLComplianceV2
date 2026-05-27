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

        var signingKey = configuration["AUTH_SIGNING_KEY"] ?? options.SigningKey;
        if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
        {
            services.AddAuthorization();
            return services;
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
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
