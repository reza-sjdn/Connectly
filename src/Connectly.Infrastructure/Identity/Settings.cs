namespace Connectly.Infrastructure.Identity;

public class JwtSettings
{
    public string Issuer { get; set; } = "Connectly";
    public string Audience { get; set; } = "Connectly.Client";
    public string Secret { get; set; } = string.Empty;   // >= 32 chars; use user-secrets / env var
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 14;
}
