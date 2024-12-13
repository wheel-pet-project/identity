using Core.Domain.RefreshTokenAggregate;
using Core.Ports.Postgres.Repositories;
using Dapper;
using Infrastructure.Settings;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class RefreshTokenRepository(
    DbSession session,
    PostgresRetryPolicy retryPolicy) : IRefreshTokenRepository
{
    public async Task Add(RefreshToken refreshToken)
    {
        var sql = @"
INSERT INTO refresh_token_info (id, account_id, is_revoked, issue_datetime, expires_at)
VALUES (@id, @accountId, @isRevoked, @issueDateTime, @expiresAt);";
        
        var command = new CommandDefinition(sql, new
        {
            id = refreshToken.Id, accountId = refreshToken.AccountId, isRevoked = refreshToken.IsRevoked,
            issueDateTime = refreshToken.IssueDateTime, expiresAt = refreshToken.ExpiresAt
        }, session.Transaction);
        
        await session.Connection.ExecuteAsync(command);
    }

    public async Task<RefreshToken?> GetNotRevokedToken(Guid refreshTokenId)
    {
        var sql = @"
SELECT id, account_id AS AccountId, is_revoked AS IsRevoked, issue_datetime AS IssueDateTime, 
       expires_at AS ExpiresAt
FROM refresh_token_info 
WHERE id = @refreshTokenId AND is_revoked = false
LIMIT 1";
        
        var command = new CommandDefinition(sql, new { refreshTokenId }, session.Transaction);
        return await retryPolicy.ExecuteAsync(() => session.Connection
            .QuerySingleOrDefaultAsync<RefreshToken>(command));
    }

    public async Task UpdateRevokeStatus(RefreshToken refreshToken)
    {
        var sql = @"
UPDATE refresh_token_info
SET is_revoked = @isRevoked
WHERE id = @id";

        var command = new CommandDefinition(sql, new { isRevoked = refreshToken.IsRevoked, id = refreshToken.Id },
            session.Transaction);

        await session.Connection.ExecuteAsync(command);
    }

    public async Task AddTokenAndRevokeOldToken(
        RefreshToken newRefreshToken, RefreshToken oldRefreshToken)
    {
        var isSuccess = false;
        
        var sqlForRevoking = @"
UPDATE refresh_token_info
SET is_revoked = true
WHERE id = @oldId";

        var sqlForAdding = @"
INSERT INTO refresh_token_info (id, account_id, is_revoked, issue_datetime, expires_at)
VALUES (@newId, @accountId, @isRevoked, @issueDateTime, @expiresAt);";

        var revokeCommand =
            new CommandDefinition(sqlForRevoking, new { oldId = oldRefreshToken.Id }, session.Transaction);
        var addCommand = new CommandDefinition(sqlForAdding, new
        {
            newId = newRefreshToken.Id, accountId = newRefreshToken.AccountId, isRevoked = oldRefreshToken.IsRevoked,
            issueDateTime = newRefreshToken.IssueDateTime, expiresAt = newRefreshToken.ExpiresAt
        }, session.Transaction);
        
        await session.Connection.ExecuteAsync(revokeCommand);
        await session.Connection.ExecuteAsync(addCommand);
    }
}