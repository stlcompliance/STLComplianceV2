using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace NexArr.Api.Services;

public sealed class MfaService
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public string GenerateSecret()
    {
        Span<byte> bytes = stackalloc byte[20];
        RandomNumberGenerator.Fill(bytes);
        return Base32Encode(bytes);
    }

    public IReadOnlyList<string> GenerateRecoveryCodes(int count = 8)
    {
        var codes = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            var bytes = new byte[10];
            RandomNumberGenerator.Fill(bytes);
            codes.Add(FormatRecoveryCode(Base32Encode(bytes)));
        }

        return codes;
    }

    public IReadOnlyList<string> HashRecoveryCodes(IEnumerable<string> recoveryCodes) =>
        recoveryCodes
            .Select(HashRecoveryCode)
            .ToArray();

    public string GenerateTotpCode(string secret, DateTimeOffset utcNow)
    {
        var step = utcNow.ToUnixTimeSeconds() / 30;
        return ComputeTotp(secret, step);
    }

    public string HashRecoveryCode(string recoveryCode)
    {
        var normalized = NormalizeRecoveryCode(recoveryCode);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool VerifyTotp(string secret, string code, DateTimeOffset utcNow)
    {
        var normalized = NormalizeCode(code);
        if (normalized.Length != 6)
        {
            return false;
        }

        var step = utcNow.ToUnixTimeSeconds() / 30;
        return Enumerable.Range(-1, 3).Any(offset =>
            ComputeTotp(secret, step + offset).Equals(normalized, StringComparison.Ordinal));
    }

    public string BuildProvisioningUri(string issuer, string accountLabel, string secret)
    {
        var escapedIssuer = Uri.EscapeDataString(issuer);
        var escapedLabel = Uri.EscapeDataString(accountLabel);
        var escapedSecret = Uri.EscapeDataString(secret);
        return $"otpauth://totp/{escapedIssuer}:{escapedLabel}?secret={escapedSecret}&issuer={escapedIssuer}&digits=6&period=30";
    }

    private static string ComputeTotp(string secret, long step)
    {
        var key = Base32Decode(secret);
        Span<byte> counterBytes = stackalloc byte[8];
        BitConverter.TryWriteBytes(counterBytes, step);
        if (BitConverter.IsLittleEndian)
        {
            counterBytes.Reverse();
        }

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes.ToArray());
        var offset = hash[^1] & 0x0f;
        var binary = ((hash[offset] & 0x7f) << 24)
            | (hash[offset + 1] << 16)
            | (hash[offset + 2] << 8)
            | hash[offset + 3];

        return (binary % 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
    }

    private static string NormalizeCode(string code) =>
        new string(code.Where(char.IsDigit).ToArray());

    private static string NormalizeRecoveryCode(string recoveryCode) =>
        new string(recoveryCode.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    private static string FormatRecoveryCode(string code)
    {
        var normalized = new string(code.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
        var chunks = Enumerable.Range(0, (normalized.Length + 3) / 4)
            .Select(index => normalized.Skip(index * 4).Take(4))
            .Where(chunk => chunk.Any())
            .Select(chunk => new string(chunk.ToArray()));
        return string.Join('-', chunks);
    }

    private static string Base32Encode(ReadOnlySpan<byte> data)
    {
        var output = new StringBuilder((data.Length * 8 + 4) / 5);
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;

            while (bitsLeft >= 5)
            {
                var index = (buffer >> (bitsLeft - 5)) & 31;
                output.Append(Base32Alphabet[index]);
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
        {
            var index = (buffer << (5 - bitsLeft)) & 31;
            output.Append(Base32Alphabet[index]);
        }

        return output.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        var normalized = new string(input.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
        var bytes = new List<byte>(normalized.Length * 5 / 8);
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var c in normalized)
        {
            var index = Base32Alphabet.IndexOf(c);
            if (index < 0)
            {
                continue;
            }

            buffer = (buffer << 5) | index;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                bytes.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                bitsLeft -= 8;
            }
        }

        return bytes.ToArray();
    }
}
