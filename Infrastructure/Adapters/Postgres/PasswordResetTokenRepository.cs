using System.Data;
using Application.Errors;
using Application.Infrastructure.Interfaces.Ports.Postgres;
using Dapper;
using FluentResults;
using Infrastructure.Adapters.Postgres.DapperModels;
using Infrastructure.Settings.Polly;

namespace Infrastructure.Adapters.Postgres;

public class PasswordResetTokenRepository(IDbConnection connection, IPostgresRetryPolicy retryPolicy) 
    : IPasswordResetTokenRepository
{
    public async Task<Result> AddPasswordResetToken(
        Guid accountId, string resetPasswordTokenHash, DateTime expiresIn)
    {
        var sql = @"
INSERT INTO password_reset_token (account_id, reset_token, is_already_applied, expires_in)
VALUES (@accountId, @resetPasswordTokenHash, false, @expiresIn);";
        
        var command = new CommandDefinition(sql, new { accountId, resetPasswordTokenHash, expiresIn });
        var affectedRows = await retryPolicy.ExecuteAsync(() => connection.ExecuteAsync(command));
        
        return affectedRows > 0
            ? Result.Ok()
            : Result.Fail(new DbError("Failed to add reset password token in db"));
    }

    public async Task<Result<(string ResetPasswordTokenHash, 
        bool IsAlreadyApplied,
        DateTime ExpiresIn)>> GetPasswordResetToken(Guid accountId)
    {
        var sql = @"
SELECT reset_token, is_already_applied, expires_in
FROM password_reset_token
WHERE account_id = @accountId
LIMIT 1";
        
        var command = new CommandDefinition(sql, new { accountId });

        var result = await retryPolicy.ExecuteAsync(() => 
            connection.QuerySingleOrDefaultAsync<PasswordResetTokenModel>(command));
        
        return result is not null
            ? Result.Ok((result.reset_token, result.is_already_applied, result.expires_in))
            : Result.Fail(new NotFound("Reset password token not found"));
    }

    public async Task<Result> MarkPasswordResetTokenAsApplied(Guid accountId)
    {
        var sql = @"
UPDATE password_reset_token
SET is_already_applied = true
WHERE account_id = @accountId";
        
        var command = new CommandDefinition(sql, new { accountId });
        
        var affectedRows = await retryPolicy.ExecuteAsync(() => connection.ExecuteAsync(command));
        
        return affectedRows > 0
            ? Result.Ok()
            : Result.Fail(new DbError("Failed to update reset password token's status in db"));
    }
}