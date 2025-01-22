namespace Infrastructure.Settings;

public class JwtOptions
{
    public required string SecretKey { get; init; }
    
    public required string Issuer { get; init; }
    
    public required int AccessTokenExpirationMinutes { get; init; }
    
    public required int RefreshTokenExpirationDays { get; init; }
}