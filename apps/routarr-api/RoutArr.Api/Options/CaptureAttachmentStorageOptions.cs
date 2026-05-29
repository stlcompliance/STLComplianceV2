namespace RoutArr.Api.Options;

public sealed class CaptureAttachmentStorageOptions
{
    public const string SectionName = "CaptureAttachmentStorage";

    public string RootPath { get; set; } = "data/routarr-capture-attachments";
}
