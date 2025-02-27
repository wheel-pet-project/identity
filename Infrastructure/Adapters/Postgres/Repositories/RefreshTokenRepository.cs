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
        var command = new CommandDefinition(_addSql, new
        {
            id = refreshToken.Id, accountId = refreshToken.AccountId, isRevoked = refreshToken.IsRevoked,
            issueDateTime = refreshToken.IssueDateTime, expiresAt = refreshToken.ExpiresAt
        }, session.Transaction);

        await session.Connection.ExecuteAsync(command);
    }

    public async Task<RefreshToken?> GetNotRevokedToken(Guid refreshTokenId)
    {
        var command = new CommandDefinition(_getNotRevokedSql, new { refreshTokenId }, session.Transaction);
        return await retryPolicy.ExecuteAsync(() => session.Connection
            .QuerySingleOrDefaultAsync<RefreshToken>(command));
    }

    public async Task<List<RefreshToken>> GetNotRevokedTokensByAccountId(Guid accountId)
    {
        var command = new CommandDefinition(_getNotRevokedTokensByAccountIdSql, new { accountId }, session.Transaction);
        var tokens = await retryPolicy.ExecuteAsync(() => session.Connection.QueryAsync<RefreshToken>(command));

        var tokensList = tokens.AsList();
        return tokensList;
    }

    public async Task UpdateRevokeStatus(RefreshToken refreshToken)
    {
        var command = new CommandDefinition(_updateRevokeStatus,
            new { isRevoked = refreshToken.IsRevoked, id = refreshToken.Id },
            session.Transaction);

        await session.Connection.ExecuteAsync(command);
    }

    public async Task AddTokenAndRevokeOldToken(
        RefreshToken newRefreshToken,
        RefreshToken oldRefreshToken)
    {
        var revokeCommand =
            new CommandDefinition(_revokeTokenSql, new { oldId = oldRefreshToken.Id }, session.Transaction);
        var addCommand = new CommandDefinition(_addSql, new
        {
            newRefreshToken.Id, accountId = newRefreshToken.AccountId, isRevoked = oldRefreshToken.IsRevoked,
            issueDateTime = newRefreshToken.IssueDateTime, expiresAt = newRefreshToken.ExpiresAt
        }, session.Transaction);

        await session.Connection.ExecuteAsync(revokeCommand);
        await session.Connection.ExecuteAsync(addCommand);
    }

    private readonly string _addSql =
        """
        INSERT INTO refresh_token_info (id, account_id, is_revoked, issue_datetime, expires_at)
        VALUES (@id, @accountId, @isRevoked, @issueDateTime, @expiresAt)
        """;

    private readonly string _revokeTokenSql =
        """
        UPDATE refresh_token_info
        SET is_revoked = true
        WHERE id = @oldId
        """;

    private readonly string _getNotRevokedSql =
        """
        SELECT id, account_id AS AccountId, is_revoked AS IsRevoked, issue_datetime AS IssueDateTime, 
               expires_at AS ExpiresAt
        FROM refresh_token_info 
        WHERE id = @refreshTokenId AND is_revoked = false
        LIMIT 1
        """;

    private readonly string _getNotRevokedTokensByAccountIdSql =
        """
        SELECT id, account_id AS AccountId, is_revoked AS IsRevoked, issue_datetime AS IssueDateTime, 
               expires_at AS ExpiresAt
        FROM refresh_token_info 
        WHERE account_id = @accountId AND is_revoked = false
        """;

    private readonly string _updateRevokeStatus =
        """
        UPDATE refresh_token_info
        SET is_revoked = @isRevoked
        WHERE id = @id
        """;
}