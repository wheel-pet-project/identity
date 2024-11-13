using FluentResults;

namespace Application.Infrastructure.Interfaces.Ports.Postgres;

public interface IPasswordResetTokenRepository
{
    Task<Result> AddPasswordResetToken(
        Guid accountId, string resetPasswordTokenHash, DateTime expiresIn);

    Task<Result<(string ResetPasswordTokenHash, bool IsAlreadyApplied, DateTime ExpiresIn)>> 
        GetPasswordResetToken(Guid accountId);

    Task<Result> MarkPasswordResetTokenAsApplied(Guid accountId);
}