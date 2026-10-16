namespace ClinicManagement.Infrastructure.Options;

/// <summary>Bound from the "Jwt" section of appsettings.json (see API/appsettings.json, Phase 6).</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
