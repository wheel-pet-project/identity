namespace Core.Infrastructure.Interfaces.PasswordHasher;

public interface IHasher
{
    string GenerateHash(string text);
    
    bool VerifyHash(string text, string hash);
}