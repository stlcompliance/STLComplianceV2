using Microsoft.Extensions.Options;
using NexArr.Api.Contracts;
using STLCompliance.Shared.Auth;

namespace NexArr.Api.Services;

public sealed class ServiceTokenDiscoveryService(
    IConfiguration configuration,
    IOptions<StlServiceTokenOptions> serviceTokenOptions)
{
    public ServiceTokenJwksResponse GetJwks()
    {
        var key = StlServiceTokenKeyMaterial.CreatePublicJwk(configuration, serviceTokenOptions.Value);
        if (key is null)
        {
            return new ServiceTokenJwksResponse([]);
        }

        return new ServiceTokenJwksResponse(
        [
            new ServiceTokenJwkResponse(
                key.Kty,
                key.Use,
                key.Kid,
                key.Alg,
                key.N,
                key.E)
        ]);
    }

    public ServiceTokenDiscoveryResponse GetDiscovery(HttpRequest request)
    {
        var options = serviceTokenOptions.Value;
        var publicKeyAvailable = StlServiceTokenKeyMaterial.IsPublicKeyAvailable(configuration, options);
        var baseUri = $"{request.Scheme}://{request.Host}";
        return new ServiceTokenDiscoveryResponse(
            StlServiceTokenKeyMaterial.ResolveIssuer(configuration, options),
            StlServiceTokenKeyMaterial.ResolveAudience(configuration, options),
            $"{baseUri}/api/v1/.well-known/jwks.json",
            publicKeyAvailable ? ["RS256"] : ["HS256"],
            publicKeyAvailable);
    }
}
