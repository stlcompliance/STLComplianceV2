using Microsoft.Extensions.Configuration;

namespace STLCompliance.Shared.Integration;

public static class StlHandoffConfiguration
{
    public static string? ResolveServiceToken(IConfiguration configuration) =>
        configuration["Handoff__ServiceToken"] ?? configuration["Handoff:ServiceToken"];
}
