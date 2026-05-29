using System.Globalization;
using System.Text.RegularExpressions;

namespace MaintainArr.Api.Services;

public static partial class VoiceNumericNormalizer
{
    private static readonly Dictionary<string, decimal> WordValues = new(StringComparer.OrdinalIgnoreCase)
    {
        ["zero"] = 0,
        ["one"] = 1,
        ["two"] = 2,
        ["three"] = 3,
        ["four"] = 4,
        ["five"] = 5,
        ["six"] = 6,
        ["seven"] = 7,
        ["eight"] = 8,
        ["nine"] = 9,
        ["ten"] = 10,
        ["eleven"] = 11,
        ["twelve"] = 12,
        ["thirteen"] = 13,
        ["fourteen"] = 14,
        ["fifteen"] = 15,
        ["sixteen"] = 16,
        ["seventeen"] = 17,
        ["eighteen"] = 18,
        ["nineteen"] = 19,
        ["twenty"] = 20,
        ["thirty"] = 30,
        ["forty"] = 40,
        ["fifty"] = 50,
        ["sixty"] = 60,
        ["seventy"] = 70,
        ["eighty"] = 80,
        ["ninety"] = 90,
        ["hundred"] = 100,
        ["thousand"] = 1000,
    };

    public static NormalizeVoiceNumericResult Normalize(string? transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return new NormalizeVoiceNumericResult(null, null, false);
        }

        var lowered = transcript.Trim().ToLowerInvariant();
        var pointParts = PointSplitPattern().Split(lowered, 2);
        if (pointParts.Length == 2)
        {
            if (TryParseSpokenNumber(CleanTranscript(pointParts[0]), out var whole)
                && TryParseSpokenNumber(CleanTranscript(pointParts[1]), out var fraction))
            {
                var fractionDigits = fraction.ToString(CultureInfo.InvariantCulture);
                var combinedText = $"{whole.ToString(CultureInfo.InvariantCulture)}.{fractionDigits}";
                if (decimal.TryParse(combinedText, NumberStyles.Number, CultureInfo.InvariantCulture, out var combined))
                {
                    return new NormalizeVoiceNumericResult(combined, combinedText, true);
                }
            }
        }

        var cleaned = CleanTranscript(lowered);
        if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var direct))
        {
            return new NormalizeVoiceNumericResult(direct, direct.ToString(CultureInfo.InvariantCulture), true);
        }

        if (TryParseSpokenNumber(cleaned, out var spoken))
        {
            return new NormalizeVoiceNumericResult(spoken, spoken.ToString(CultureInfo.InvariantCulture), true);
        }

        return new NormalizeVoiceNumericResult(null, cleaned, false);
    }

    private static string CleanTranscript(string transcript)
    {
        var lowered = transcript.Trim().ToLowerInvariant();
        lowered = NonNumericWordPattern().Replace(lowered, " ");
        lowered = WhitespacePattern().Replace(lowered, " ").Trim();
        return lowered;
    }

    private static bool TryParseSpokenNumber(string cleaned, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return false;
        }

        var tokens = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        decimal total = 0;
        decimal current = 0;
        var sawToken = false;

        foreach (var token in tokens)
        {
            if (decimal.TryParse(token, NumberStyles.Number, CultureInfo.InvariantCulture, out var numericToken))
            {
                sawToken = true;
                current += numericToken;
                continue;
            }

            if (!WordValues.TryGetValue(token, out var wordValue))
            {
                return false;
            }

            sawToken = true;
            if (wordValue == 100)
            {
                current = current == 0 ? 100 : current * 100;
            }
            else if (wordValue == 1000)
            {
                current = current == 0 ? 1000 : current * 1000;
            }
            else
            {
                current += wordValue;
            }
        }

        if (!sawToken)
        {
            return false;
        }

        total += current;
        value = total;
        return true;
    }

    [GeneratedRegex(@"\bpoint\b", RegexOptions.CultureInvariant)]
    private static partial Regex PointSplitPattern();

    [GeneratedRegex(@"\b(and|a|the|is|equals|about|approximately|roughly|around)\b", RegexOptions.CultureInvariant)]
    private static partial Regex NonNumericWordPattern();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespacePattern();
}

public sealed record NormalizeVoiceNumericResult(decimal? Value, string? NormalizedText, bool Understood);
