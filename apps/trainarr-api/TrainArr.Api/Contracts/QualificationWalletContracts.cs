namespace TrainArr.Api.Contracts;

public sealed record QualificationWalletCredentialResponse(
    Guid QualificationIssueId,
    Guid StaffarrPersonId,
    string QualificationKey,
    string QualificationName,
    string Status,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset GeneratedAt,
    string CredentialToken,
    string VerificationUrl,
    string DisplayLabel);

public sealed record QualificationWalletVerificationRequest(
    string CredentialToken);

public sealed record QualificationWalletVerificationResponse(
    DateTimeOffset VerifiedAt,
    bool IsValid,
    string Message,
    QualificationWalletCredentialResponse? Credential,
    QualificationPointInTimeReportResponse? Report);
