using Application.Infrastructure.Interfaces.PasswordHasher;

namespace Infrastructure.Hasher;

public class Hasher : IHasher
{
    public string GenerateHash(string text) => 
        BCrypt.Net.BCrypt.EnhancedHashPassword(text);

    public bool VerifyHash(string text, string hash) => 
        BCrypt.Net.BCrypt.EnhancedVerify(text, hash);
}