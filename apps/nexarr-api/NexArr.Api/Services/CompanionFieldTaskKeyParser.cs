namespace NexArr.Api.Services;

public static class CompanionFieldTaskKeyParser
{
    public static bool TryParse(string taskKey, out CompanionFieldTaskReference reference)
    {
        reference = default!;
        if (string.IsNullOrWhiteSpace(taskKey))
        {
            return false;
        }

        var parts = taskKey.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!Guid.TryParse(parts[2], out var resourceId))
        {
            return false;
        }

        reference = new CompanionFieldTaskReference(
            parts[0].ToLowerInvariant(),
            parts[1].ToLowerInvariant(),
            resourceId);
        return true;
    }
}

public sealed record CompanionFieldTaskReference(string ProductKey, string ResourceType, Guid ResourceId);
