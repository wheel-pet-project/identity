namespace Infrastructure.Settings;

public class JwtOptions
{
    public string SecretKey { get; set; }
    
    public string Issuer { get; set; }
    
    public int Expiration { get; set; }
}