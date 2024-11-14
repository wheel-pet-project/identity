using System.Data;
using Application.Errors;
using Application.Infrastructure.Interfaces.Ports.Postgres;
using Dapper;
using FluentResults;
using Infrastructure.Adapters.Postgres.DapperModels;
using Infrastructure.Settings.Polly;

namespace Infrastructure.Adapters.Postgres;

public class RefreshTokenRepository(IDbConnection connection, IPostgresRetryPolicy retryPolicy) 
    : IRefreshTokenRepository
{
    public async Task<Result> AddRefreshTokenInfo(Guid refreshTokenId, Guid accountId)
    {
        var sql = @"
INSERT INTO refresh_token_info (id, account_id, is_revoked, issue_date)
VALUES (@refreshTokenId, @accountId, false, @issueDate);";
        
        var command = new CommandDefinition(sql, new
        {
            accountId, refreshTokenId, issueDate = DateTime.UtcNow
        });
        var affectedRows = await retryPolicy.ExecuteAsync(() => connection.ExecuteAsync(command));
        
        return affectedRows > 0
            ? Result.Ok()
            : Result.Fail(new DbError("Saving refresh token info failed"));
    }

    public async Task<Result<(Guid AccountId, bool IsRevoked)>> GetRefreshTokenInfo(Guid refreshTokenId)
    {
        var sql = @"
SELECT account_id, is_revoked
FROM refresh_token_info
WHERE id = @refreshTokenId
LIMIT 1";
        
        var command = new CommandDefinition(sql, new { refreshTokenId });
        var result = await retryPolicy.ExecuteAsync(() => connection
            .QuerySingleOrDefaultAsync<RefreshTokenInfoModel>(command));

        return result is not null
            ? Result.Ok((result.account_id, result.is_revoked))
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
        Guid newRefreshTokenId, Guid accountId, Guid oldRefreshTokenId)
    {
        var isSuccess = true;
        
        var sqlForRevoking = @"
UPDATE refresh_token_info
SET is_revoked = true
WHERE id = @oldRefreshTokenId";

        var sqlForAdding = @"
INSERT INTO refresh_token_info (id, account_id, is_revoked, issue_date)
VALUES (@newRefreshTokenId, @accountId, false, @issueDate);";

        await retryPolicy.ExecuteAsync(async () =>
        {
            connection.Open();
            using var transaction = connection.BeginTransaction();

            var revokeCommand = new CommandDefinition(sqlForRevoking, new { oldRefreshTokenId }, transaction);
            var addCommand = new CommandDefinition(sqlForAdding, new
            {
                newRefreshTokenId, accountId, issueDate = DateTime.UtcNow
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