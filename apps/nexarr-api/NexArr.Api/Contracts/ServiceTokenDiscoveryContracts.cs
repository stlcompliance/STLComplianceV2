namespace NexArr.Api.Contracts;

public sealed record ServiceTokenJwkResponse(
    string Kty,
    string Use,
    string Kid,
    string Alg,
    string N,
    string E);

public sealed record ServiceTokenJwksResponse(IReadOnlyList<ServiceTokenJwkResponse> Keys);

public sealed record ServiceTokenDiscoveryResponse(
    string Issuer,
    string Audience,
    string JwksUri,
    IReadOnlyList<string> SupportedAlgorithms,
    bool PublicKeyAvailable);
