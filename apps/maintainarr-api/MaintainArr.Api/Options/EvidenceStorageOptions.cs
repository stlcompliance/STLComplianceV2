namespace MaintainArr.Api.Options;

public sealed class EvidenceStorageOptions
{
    public const string SectionName = "EvidenceStorage";

    public string RootPath { get; set; } = "data/maintainarr-evidence";
}
