namespace NexArr.Api.Options;

public sealed class CompanionWebPushOptions
{
    public const string SectionName = "CompanionWebPush";

    public string? Subject { get; set; }

    public string? PublicKey { get; set; }

    public string? PrivateKey { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Subject)
        && !string.IsNullOrWhiteSpace(PublicKey)
        && !string.IsNullOrWhiteSpace(PrivateKey);
}
