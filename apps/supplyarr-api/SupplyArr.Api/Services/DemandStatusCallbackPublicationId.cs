using System.Security.Cryptography;
using System.Text;

namespace SupplyArr.Api.Services;

internal static class DemandStatusCallbackPublicationId
{
    public static Guid Create(
        Guid tenantId,
        Guid demandRefId,
        string eventType,
        Guid sourceRecordId)
    {
        var input = $"{tenantId:N}:{demandRefId:N}:{eventType}:{sourceRecordId:N}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash.AsSpan(0, 16));
    }
}
