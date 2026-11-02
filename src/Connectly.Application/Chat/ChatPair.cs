namespace Connectly.Application.Chat;

internal static class ChatPair
{
    /// <summary>Canonical (A, B) order so one pair of users maps to exactly one conversation row.</summary>
    public static (Guid A, Guid B) Order(Guid x, Guid y) => x.CompareTo(y) < 0 ? (x, y) : (y, x);
}
