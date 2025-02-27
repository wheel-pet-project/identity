using Core.Domain.PasswordRecoverTokenAggregate;
using Core.Ports.Postgres.Repositories;
using Dapper;
using Infrastructure.Settings;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class PasswordRecoverTokenRepository(
    DbSession session,
    PostgresRetryPolicy retryPolicy) : IPasswordRecoverTokenRepository
{
    public async Task Add(PasswordRecoverToken token)
    {
        var command = new CommandDefinition(_addSql,
            new
            {
                id = token.Id, accountId = token.AccountId, recoverTokenHash = token.RecoverTokenHash,
                isAlreadyApplied = token.IsAlreadyApplied, expiresAt = token.ExpiresAt
            }, session.Transaction);

        await session.Connection.ExecuteAsync(command);
    }

    public async Task<PasswordRecoverToken?> Get(Guid accountId)
    {
        var command = new CommandDefinition(_getSql, new { accountId }, session.Transaction);

        return await retryPolicy.ExecuteAsync(() =>
            session.Connection.QuerySingleOrDefaultAsync<PasswordRecoverToken>(command));
    }

    public async Task UpdateAppliedStatus(PasswordRecoverToken token)
    {
        var command = new CommandDefinition(_updateAppliedStatusSql,
            new { isAlreadyApplied = token.IsAlreadyApplied, accountId = token.AccountId }, session.Transaction);

        await session.Connection.ExecuteAsync(command);
    }

    private readonly string _addSql =
        """
        INSERT INTO password_recover_token (id, account_id, recover_token_hash, is_already_applied, expires_at)
        VALUES (@id, @accountId, @recoverTokenHash, @isAlreadyApplied, @expiresAt)
        """;

    private readonly string _getSql =
        """
        SELECT id, account_id AS accountId, recover_token_hash AS recoverTokenHash, 
               is_already_applied AS isAlreadyApplied, expires_at as ExpiresAt
        FROM password_recover_token
        WHERE account_id = @accountId
        LIMIT 1
        """;

    private readonly string _updateAppliedStatusSql =
        """
        UPDATE password_recover_token
        SET is_already_applied = @isAlreadyApplied
        WHERE account_id = @accountId
        """;
}