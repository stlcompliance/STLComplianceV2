namespace NexArr.Api.Services;

internal static class PlatformUserStatusResolver
{
    public static string Resolve(
        bool isActive,
        bool canLogin,
        bool? isEmailVerified,
        bool requiresPasswordChange,
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

        if (requiresPasswordChange)
        {
            return "password_change_required";
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
