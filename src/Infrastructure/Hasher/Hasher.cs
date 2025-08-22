using Core.Infrastructure.Interfaces.PasswordHasher;

namespace Infrastructure.Hasher;

public class Hasher : IHasher
{
    public string GenerateHash(string text)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(text);
    }

    public bool VerifyHash(string text, string hash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(text, hash);
    }
}