namespace NexArr.Api.Services;

public static class FieldCompanionFieldTaskKeyParser
{
    public static bool TryParse(string taskKey, out FieldCompanionFieldTaskReference reference)
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

        reference = new FieldCompanionFieldTaskReference(
            parts[0].ToLowerInvariant(),
            parts[1].ToLowerInvariant(),
            resourceId);
        return true;
    }
}

public sealed record FieldCompanionFieldTaskReference(string ProductKey, string ResourceType, Guid ResourceId);
