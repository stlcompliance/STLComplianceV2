using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace NexArr.Api.Endpoints;

internal static class EndpointPrefixAliases
{
    public static void MapLegacyAndCanonical(
        this WebApplication app,
        string legacyPrefix,
        string canonicalPrefix,
        Action<RouteGroupBuilder, bool> configure)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(legacyPrefix);
        ArgumentNullException.ThrowIfNull(canonicalPrefix);
        ArgumentNullException.ThrowIfNull(configure);

        var legacyGroup = app.MapGroup(legacyPrefix).ExcludeFromDescription();
        configure(legacyGroup, false);

        if (!string.Equals(legacyPrefix, canonicalPrefix, StringComparison.Ordinal))
        {
            var canonicalGroup = app.MapGroup(canonicalPrefix);
            configure(canonicalGroup, true);
        }
    }
}
