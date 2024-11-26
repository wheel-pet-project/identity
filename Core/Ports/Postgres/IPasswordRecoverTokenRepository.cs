using FluentResults;

namespace Core.Ports.Postgres;

public interface IPasswordRecoverTokenRepository
{
    Task<Result> AddPasswordRecoverToken(
        Guid accountId, string resetPasswordTokenHash, DateTime expiresIn);

    Task<Result<(string ResetPasswordTokenHash, bool IsAlreadyApplied, DateTime ExpiresIn)>> 
        GetPasswordRecoverToken(Guid accountId);

    Task<Result> MarkPasswordRecoverTokenAsApplied(Guid accountId);
}