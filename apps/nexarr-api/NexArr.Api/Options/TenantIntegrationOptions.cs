namespace NexArr.Api.Options;

public sealed class TenantIntegrationOptions
{
    public const string SectionName = "TenantIntegrations";

    public string EncryptionKey { get; set; } = string.Empty;

    public int WorkerBatchSize { get; set; } = 25;

    public int RetryIntervalMinutes { get; set; } = 15;

    public int MaxRetryAttempts { get; set; } = 5;
}
