using System.Globalization;
using System.Text.RegularExpressions;

namespace ComplianceCore.Api.Services;

public sealed record RetentionWindowEvaluationResult(
    bool Passed,
    string EvaluatedValue,
    int? DaysRemaining,
    bool IsDueSoon);

public static class RetentionWindowRules
{
    private static readonly Regex RetentionDurationRegex = new(
        @"^(?<value>\d+)\s*(?<unit>day|days|week|weeks|month|months|year|years)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static RetentionWindowEvaluationResult EvaluateCurrent(
        DateTimeOffset? assertedAt,
        DateTimeOffset? effectiveAt,
        DateTimeOffset? expiresAt,
        string? retentionPeriod,
        string? value,
        DateTimeOffset now,
        int dueSoonThresholdDays = 30)
    {
        var anchor = effectiveAt ?? assertedAt;
        var parsedRetentionDays = TryParseRetentionDays(retentionPeriod);

        if (expiresAt.HasValue)
        {
            return EvaluateUntil(expiresAt.Value, now, dueSoonThresholdDays);
        }

        if (anchor.HasValue && parsedRetentionDays.HasValue)
        {
            return EvaluateUntil(anchor.Value.AddDays(parsedRetentionDays.Value), now, dueSoonThresholdDays);
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var instant))
        {
            return EvaluateUntil(instant, now, dueSoonThresholdDays);
        }

        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            var endOfDayUtc = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            return EvaluateUntil(new DateTimeOffset(endOfDayUtc), now, dueSoonThresholdDays);
        }

        return new RetentionWindowEvaluationResult(false, value ?? string.Empty, null, false);
    }

    public static int? TryParseRetentionDays(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var days))
        {
            return days;
        }

        var match = RetentionDurationRegex.Match(trimmed);
        if (!match.Success)
        {
            return null;
        }

        var amount = int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
        return match.Groups["unit"].Value.ToLowerInvariant() switch
        {
            "day" or "days" => amount,
            "week" or "weeks" => amount * 7,
            "month" or "months" => amount * 30,
            "year" or "years" => amount * 365,
            _ => null,
        };
    }

    private static RetentionWindowEvaluationResult EvaluateUntil(
        DateTimeOffset currentUntil,
        DateTimeOffset now,
        int dueSoonThresholdDays)
    {
        var remainingDays = (int)Math.Ceiling((currentUntil - now).TotalDays);
        if (remainingDays >= 0)
        {
            var label = remainingDays <= dueSoonThresholdDays
                ? $"current (due in {remainingDays} day{(remainingDays == 1 ? string.Empty : "s")})"
                : $"current (due in {remainingDays} day{(remainingDays == 1 ? string.Empty : "s")})";

            return new RetentionWindowEvaluationResult(true, label, remainingDays, remainingDays <= dueSoonThresholdDays);
        }

        var overdueDays = Math.Abs(remainingDays);
        return new RetentionWindowEvaluationResult(
            false,
            $"expired {overdueDays} day{(overdueDays == 1 ? string.Empty : "s")} ago",
            remainingDays,
            false);
    }
}
