using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace STLCompliance.Shared.Print;

public sealed class StlPlainTextPdfRenderer : IPdfRenderer
{
    private const int MaxCharactersPerLine = 92;
    private const int LinesPerPage = 46;
    private const double PageWidth = 612d;
    private const double PageHeight = 792d;
    private const double LeftMargin = 54d;
    private const double TopMargin = 744d;
    private const double LineHeight = 14d;

    public Task<StlGeneratedPrintFile> RenderPdfAsync(
        StlRenderablePrintDocument document,
        CancellationToken cancellationToken)
    {
        var text = HtmlToPlainText(document.Html);
        var lines = WrapLines(text, MaxCharactersPerLine);
        if (lines.Count == 0)
        {
            lines.Add(document.DocumentTitle);
        }

        var bytes = BuildPdf(lines, cancellationToken);
        var fileName = EnsurePdfFileName(document.FileName);

        return Task.FromResult(
            new StlGeneratedPrintFile(
                document.DocumentTitle,
                document.SourceDisplayRef,
                document.TemplateKey,
                document.TemplateVersion,
                fileName,
                "application/pdf",
                bytes,
                document.Warnings,
                document.MissingRequirements,
                ContentHash: ComputeContentHash(bytes)));
    }

    private static byte[] BuildPdf(IReadOnlyList<string> lines, CancellationToken cancellationToken)
    {
        var pages = Paginate(lines, LinesPerPage);
        var objectCount = 3 + (pages.Count * 2);
        var pageObjectNumbers = Enumerable
            .Range(0, pages.Count)
            .Select(index => 4 + (index * 2))
            .ToArray();

        var objects = new Dictionary<int, byte[]>
        {
            [1] = EncodeAscii("<< /Type /Catalog /Pages 2 0 R >>"),
            [2] = EncodeAscii(
                $"<< /Type /Pages /Count {pages.Count} /Kids [{string.Join(" ", pageObjectNumbers.Select(number => $"{number} 0 R"))}] >>"),
            [3] = EncodeAscii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"),
        };

        for (var index = 0; index < pages.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pageObjectNumber = pageObjectNumbers[index];
            var contentObjectNumber = pageObjectNumber + 1;
            var stream = BuildPageStream(pages[index]);

            objects[pageObjectNumber] = EncodeAscii(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {FormatNumber(PageWidth)} {FormatNumber(PageHeight)}] /Resources << /Font << /F1 3 0 R >> >> /Contents {contentObjectNumber} 0 R >>");
            objects[contentObjectNumber] = EncodeAscii(
                $"<< /Length {stream.Length} >>\nstream\n{stream}\nendstream");
        }

        using var streamBuilder = new MemoryStream();
        WriteAscii(streamBuilder, "%PDF-1.4\n");

        var offsets = new int[objectCount + 1];
        for (var objectNumber = 1; objectNumber <= objectCount; objectNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            offsets[objectNumber] = checked((int)streamBuilder.Position);
            WriteAscii(streamBuilder, $"{objectNumber} 0 obj\n");
            streamBuilder.Write(objects[objectNumber], 0, objects[objectNumber].Length);
            WriteAscii(streamBuilder, "\nendobj\n");
        }

        var xrefOffset = checked((int)streamBuilder.Position);
        WriteAscii(streamBuilder, $"xref\n0 {objectCount + 1}\n");
        WriteAscii(streamBuilder, "0000000000 65535 f \n");
        for (var objectNumber = 1; objectNumber <= objectCount; objectNumber++)
        {
            WriteAscii(streamBuilder, $"{offsets[objectNumber]:D10} 00000 n \n");
        }

        WriteAscii(streamBuilder, $"trailer\n<< /Size {objectCount + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");
        return streamBuilder.ToArray();
    }

    private static string BuildPageStream(IReadOnlyList<string> lines)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BT");
        builder.AppendLine("/F1 11 Tf");
        builder.AppendLine($"{FormatNumber(LineHeight)} TL");
        builder.AppendLine(
            $"1 0 0 1 {FormatNumber(LeftMargin)} {FormatNumber(TopMargin)} Tm");

        for (var index = 0; index < lines.Count; index++)
        {
            var line = EscapePdfText(ToAscii(lines[index]));
            builder.Append('(').Append(line).AppendLine(") Tj");
            if (index < lines.Count - 1)
            {
                builder.AppendLine("T*");
            }
        }

        builder.Append("ET");
        return builder.ToString();
    }

    private static IReadOnlyList<IReadOnlyList<string>> Paginate(
        IReadOnlyList<string> lines,
        int linesPerPage)
    {
        var pages = new List<IReadOnlyList<string>>();
        for (var index = 0; index < lines.Count; index += linesPerPage)
        {
            pages.Add(lines.Skip(index).Take(linesPerPage).ToArray());
        }

        if (pages.Count == 0)
        {
            pages.Add([" "]);
        }

        return pages;
    }

    private static List<string> WrapLines(string text, int width)
    {
        var wrapped = new List<string>();
        foreach (var rawLine in text.Split('\n'))
        {
            var normalized = Regex.Replace(rawLine.TrimEnd(), "\\s+", " ");
            if (string.IsNullOrWhiteSpace(normalized))
            {
                if (wrapped.Count == 0 || !string.IsNullOrEmpty(wrapped[^1]))
                {
                    wrapped.Add(string.Empty);
                }
                continue;
            }

            var remaining = normalized.Trim();
            while (remaining.Length > width)
            {
                var splitAt = remaining.LastIndexOf(' ', Math.Min(width, remaining.Length - 1));
                if (splitAt <= 0)
                {
                    splitAt = width;
                }

                wrapped.Add(remaining[..splitAt].TrimEnd());
                remaining = remaining[splitAt..].TrimStart();
            }

            wrapped.Add(remaining);
        }

        while (wrapped.Count > 0 && string.IsNullOrEmpty(wrapped[^1]))
        {
            wrapped.RemoveAt(wrapped.Count - 1);
        }

        return wrapped;
    }

    private static string HtmlToPlainText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var normalized = html
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("</li>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<li>", "- ", StringComparison.OrdinalIgnoreCase);

        normalized = Regex.Replace(
            normalized,
            @"<br\s*/?>|</p>|</div>|</section>|</article>|</header>|</footer>|</tr>|</h[1-6]>",
            "\n",
            RegexOptions.IgnoreCase);
        normalized = Regex.Replace(
            normalized,
            @"</table>|</ul>|</ol>",
            "\n\n",
            RegexOptions.IgnoreCase);
        normalized = Regex.Replace(normalized, "<[^>]+>", string.Empty, RegexOptions.Singleline);
        normalized = WebUtility.HtmlDecode(normalized);
        normalized = normalized.Replace('\u00A0', ' ');
        normalized = Regex.Replace(normalized, "\n{3,}", "\n\n");
        return normalized.Trim();
    }

    private static string EscapePdfText(string value) =>
        value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);

    private static string EnsurePdfFileName(string fileName)
    {
        var trimmed = string.IsNullOrWhiteSpace(fileName) ? "document.pdf" : fileName.Trim();
        return trimmed.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? trimmed
            : $"{trimmed}.pdf";
    }

    private static string ComputeContentHash(byte[] bytes)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ToAscii(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            builder.Append(character <= sbyte.MaxValue ? character : '?');
        }

        return builder.ToString();
    }

    private static string FormatNumber(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private static byte[] EncodeAscii(string value) => Encoding.ASCII.GetBytes(value);

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = EncodeAscii(value);
        stream.Write(bytes, 0, bytes.Length);
    }
}
