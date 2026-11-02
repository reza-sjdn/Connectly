using System.Text;

namespace Connectly.Application.Common.Helpers;

/// <summary>Opaque keyset cursor: (CreatedAtUtc ticks, Id), URL-safe base64.</summary>
public static class PageCursor
{
    public static string Encode(DateTime createdAtUtc, Guid id)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes($"{createdAtUtc.Ticks}:{id:N}"))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public static bool TryDecode(string? cursor, out DateTime createdAtUtc, out Guid id)
    {
        createdAtUtc = default;
        id = default;
        if (string.IsNullOrWhiteSpace(cursor)) return false;

        try
        {
            var b64 = cursor.Replace('-', '+').Replace('_', '/');
            b64 = b64.PadRight(b64.Length + (4 - b64.Length % 4) % 4, '=');
            var parts = Encoding.UTF8.GetString(Convert.FromBase64String(b64)).Split(':');
            if (parts.Length != 2 || !long.TryParse(parts[0], out var ticks) || !Guid.TryParseExact(parts[1], "N", out id))
                return false;
            createdAtUtc = new DateTime(ticks, DateTimeKind.Utc);
            return true;
        }
        catch (Exception e) when (e is FormatException or ArgumentException)
        {
            return false;
        }
    }
}
