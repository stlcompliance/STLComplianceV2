using RoutArr.Api.Entities;
using STLCompliance.Shared.Contracts;

namespace RoutArr.Api.Services;

public static class TripCaptureAttachmentRules
{
    public const long MaxAttachmentBytes = 10 * 1024 * 1024;

    public static byte[] DecodeContent(string contentBase64)
    {
        if (string.IsNullOrWhiteSpace(contentBase64))
        {
            return [];
        }

        var payload = contentBase64.Trim();
        var commaIndex = payload.IndexOf(',');
        if (commaIndex >= 0 && payload[..commaIndex].Contains("base64", StringComparison.OrdinalIgnoreCase))
        {
            payload = payload[(commaIndex + 1)..];
        }

        try
        {
            return Convert.FromBase64String(payload);
        }
        catch (FormatException)
        {
            throw new StlApiException(
                "trip_capture_attachment.validation",
                "Attachment content must be valid base64.",
                400);
        }
    }

    public static string NormalizeAttachmentKind(string attachmentKind)
    {
        var normalized = attachmentKind?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!TripCaptureAttachmentKinds.All.Contains(normalized))
        {
            throw new StlApiException(
                "trip_capture_attachment.invalid_kind",
                "Attachment kind must be photo, document, or signature.",
                400);
        }

        return normalized;
    }

    public static string NormalizeSubjectType(string subjectType)
    {
        var normalized = subjectType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!TripCaptureAttachmentSubjects.All.Contains(normalized))
        {
            throw new StlApiException(
                "trip_capture_attachment.invalid_subject",
                "Attachment subject must be proof or dvir.",
                400);
        }

        return normalized;
    }

    public static string NormalizeFileName(string fileName)
    {
        var trimmed = fileName?.Trim() ?? string.Empty;
        if (trimmed.Length < 1 || trimmed.Length > 255)
        {
            throw new StlApiException(
                "trip_capture_attachment.validation",
                "File name must be between 1 and 255 characters.",
                400);
        }

        return trimmed;
    }

    public static string NormalizeContentType(string contentType)
    {
        var trimmed = contentType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (trimmed.Length < 3 || trimmed.Length > 128)
        {
            throw new StlApiException(
                "trip_capture_attachment.validation",
                "Content type must be between 3 and 128 characters.",
                400);
        }

        return trimmed;
    }

    public static string? NormalizeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var trimmed = notes.Trim();
        if (trimmed.Length > 1024)
        {
            throw new StlApiException(
                "trip_capture_attachment.validation",
                "Notes must be 1024 characters or fewer.",
                400);
        }

        return trimmed;
    }

    public static void ValidateContent(byte[] contentBytes)
    {
        if (contentBytes.Length == 0)
        {
            throw new StlApiException(
                "trip_capture_attachment.validation",
                "Attachment content is required.",
                400);
        }

        if (contentBytes.Length > MaxAttachmentBytes)
        {
            throw new StlApiException(
                "trip_capture_attachment.validation",
                $"Attachment file must be {MaxAttachmentBytes / (1024 * 1024)} MB or smaller.",
                400);
        }
    }
}
