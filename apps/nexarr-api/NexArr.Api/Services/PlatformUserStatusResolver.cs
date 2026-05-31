namespace NexArr.Api.Services;

internal static class PlatformUserStatusResolver
{
    public static string Resolve(
        bool isActive,
        bool canLogin,
        bool? isEmailVerified,
        DateTimeOffset? lockedUntil,
        DateTimeOffset now)
    {
        if (!isActive)
        {
            return "disabled";
        }

        if (!canLogin)
        {
            return "invited";
        }

        if (isEmailVerified is false)
        {
            return "pending_verification";
        }

        if (lockedUntil is DateTimeOffset lockUntil && lockUntil > now)
        {
            return "locked";
        }

        return "active";
    }
}
