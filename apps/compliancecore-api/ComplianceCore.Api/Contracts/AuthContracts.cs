namespace ComplianceCore.Api.Contracts;

public sealed record RedeemHandoffRequest(string HandoffCode);

public sealed record HandoffSessionResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    Guid PersonId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantSlug,
    string TenantDisplayName,
    Guid SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    IReadOnlyList<string> LaunchableProductKeys,
    string ThemePreference,
    string? CallbackUrl);

public sealed record ComplianceCoreSessionBootstrapResponse(
    Guid UserId,
    Guid PersonId,
    Guid TenantId,
    Guid SessionId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    IReadOnlyList<string> LaunchableProductKeys,
    bool CanManageVocabulary,
    bool CanExportAuditPackage,
    bool CanEvaluateRiskScores,
    bool CanEvaluateMissingEvidenceWarnings,
    bool CanEvaluateControlEffectiveness,
    bool CanEvaluateReadinessForecast,
    bool CanReadReports,
    bool CanExportReports);

public sealed record ComplianceCoreMeResponse(
    Guid UserId,
    Guid PersonId,
    string Email,
    string DisplayName,
    Guid TenantId,
    string TenantRoleKey,
    bool IsPlatformAdmin,
    string ProductKey,
    IReadOnlyList<string> LaunchableProductKeys,
    bool CanManageVocabulary,
    bool CanExportAuditPackage,
    bool CanEvaluateRiskScores,
    bool CanEvaluateMissingEvidenceWarnings,
    bool CanEvaluateControlEffectiveness,
    bool CanEvaluateReadinessForecast,
    bool CanReadReports,
    bool CanExportReports);
