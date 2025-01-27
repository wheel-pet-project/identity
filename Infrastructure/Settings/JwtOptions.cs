namespace Infrastructure.Settings;

public class JwtOptions
{
    public required string SecretKey { get; set; }
    
    public required string Issuer { get; set; }
    
    public required int AccessTokenExpirationMinutes { get; set; }
    
    public required int RefreshTokenExpirationDays { get; set; }
}