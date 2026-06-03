namespace SupplyArr.Api.Options;

public sealed class DocumentStorageOptions
{
    public const string SectionName = "DocumentStorage";

    public string RootPath { get; set; } = "data/supplyarr-documents";
}
