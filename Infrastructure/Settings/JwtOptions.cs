namespace Infrastructure.Settings;

public class JwtOptions
{
    public string SecretKey { get; init; }
    
    public string Issuer { get; init; }
    
    public int AccessTokenExpirationMinutes { get; init; }
    
    public int RefreshTokenExpirationDays { get; init; }
}