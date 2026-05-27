namespace SupplyArr.Api.Entities;

public static class ReceivingExceptionTypes
{
    public const string Short = "short";

    public const string Over = "over";

    public const string Damage = "damage";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Short,
        Over,
        Damage
    };
}
