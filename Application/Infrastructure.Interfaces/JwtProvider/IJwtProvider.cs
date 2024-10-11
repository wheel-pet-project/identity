using Domain.AccountAggregate;

namespace Application.Infrastructure.Interfaces.JwtProvider;

public interface IJwtProvider
{
    string GenerateToken(Account account);

    Task<(bool isValid, Guid accId, int role, int status)> VerifyToken(string accessToken);
}