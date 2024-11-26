using System.Data;
using Core.Domain.RefreshTokenAggregate;
using Core.Domain.SharedKernel.Errors;
using Core.Ports.Postgres;
using Dapper;
using FluentResults;
using Infrastructure.Settings;

namespace Infrastructure.Adapters.Postgres;

public class RefreshTokenRepository(IDbConnection connection, PostgresRetryPolicy retryPolicy) 
    : IRefreshTokenRepository
{
    public async Task<Result> AddRefreshTokenInfo(RefreshToken refreshToken)
    {
        var sql = @"
INSERT INTO refresh_token_info (id, account_id, is_revoked, issue_datetime, expires_at)
VALUES (id, account_id, is_revoked, issue_datetime, expires_at);";
        
        var command = new CommandDefinition(sql, new
        {
            id = refreshToken.Id, accountId = refreshToken.AccountId, isRevoked = refreshToken.IsRevoked,
            issueDateTime = refreshToken.IssueDateTime, expires_at = refreshToken.ExpiresAt
        });
        var affectedRows = await retryPolicy.ExecuteAsync(() => connection.ExecuteAsync(command));
        
        return affectedRows > 0
            ? Result.Ok()
            : Result.Fail(new DbError("Saving refresh token info failed"));
    }

    public async Task<Result<RefreshToken>> GetRefreshTokenInfo(Guid refreshTokenId)
    {
        var sql = @"
SELECT id, account_id, is_revoked, issue_datetime, expires_at
FROM refresh_token_info 
WHERE id = @refreshTokenId
LIMIT 1";
        
        var command = new CommandDefinition(sql, new { refreshTokenId });
        var result = await retryPolicy.ExecuteAsync(() => connection
            .QuerySingleOrDefaultAsync<RefreshToken>(command));

        return result is not null
            ? Result.Ok(result)
            : Result.Fail(new NotFound("Refresh token not found"));
    }

    public async Task<Result> UpdateRefreshTokenInfo(Guid refreshTokenId, bool isRevoked)
    {
        var sql = @"
UPDATE refresh_token_info
SET is_revoked = @isRevoked
WHERE id = @refreshTokenId";
        
        var command = new CommandDefinition(sql, new { isRevoked , refreshTokenId});

        var affectedRows = await retryPolicy.ExecuteAsync(() => connection.ExecuteAsync(command));

        return affectedRows > 0
            ? Result.Ok()
            : Result.Fail(new DbError("Updating refresh token in db failed"));
    }

    public async Task<Result> AddRefreshTokenInfoAndRevokeOldRefreshToken(
        RefreshToken newRefreshToken, RefreshToken oldRefreshToken)
    {
        var isSuccess = true;
        
        var sqlForRevoking = @"
UPDATE refresh_token_info
SET is_revoked = true
WHERE id = @oldId";

        var sqlForAdding = @"
INSERT INTO refresh_token_info (id, account_id, is_revoked, issue_datetime, expires_at)
VALUES (@newId, @accountId, @isRevoked, @issueDateTime, @expiresAt);";

        await retryPolicy.ExecuteAsync(async () =>
        {
            connection.Open();
            using var transaction = connection.BeginTransaction();

            var revokeCommand = new CommandDefinition(sqlForRevoking, new { oldId = oldRefreshToken.Id }, transaction);
            var addCommand = new CommandDefinition(sqlForAdding, new
            {
                newId = newRefreshToken.Id, accountId = newRefreshToken.AccountId,
                issueDate = newRefreshToken.IssueDateTime, expiresAt = newRefreshToken.ExpiresAt
            }, transaction);

            try
            {
                await connection.ExecuteAsync(revokeCommand);
                await connection.ExecuteAsync(addCommand);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                isSuccess = false;
            }
        });
        
        return isSuccess 
            ? Result.Ok()
            : Result.Fail(new DbError("Update refresh token in db failed"));
    }
}