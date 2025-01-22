using Core.Domain.ConfirmationTokenAggregate;
using Core.Ports.Postgres.Repositories;
using Dapper;
using Infrastructure.Settings;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class ConfirmationTokenRepository(
    DbSession session,
    PostgresRetryPolicy retryPolicy) : IConfirmationTokenRepository
{
    
    public async Task Add(ConfirmationToken confirmationToken)
    {
        var createConfirmRecordCommand = new CommandDefinition(_addSql,
            new { confirmationToken.AccountId, confirmationToken.ConfirmationTokenHash }, session.Transaction);
        
        await session.Connection.ExecuteAsync(createConfirmRecordCommand);
    }

    public async Task<ConfirmationToken?> Get(Guid accountId)
    {
        var command = new CommandDefinition(_getSql, new { accountId }, session.Transaction);

        return await retryPolicy.ExecuteAsync(() =>
            session.Connection.QuerySingleOrDefaultAsync<ConfirmationToken>(command)); 
    }

    public async Task Delete(Guid accountId)
    {
        var command = new CommandDefinition(_deleteSql, new { accountId }, session.Transaction);
        await session.Connection.ExecuteAsync(command);
    }

    private readonly string _addSql =
        """
        INSERT INTO pending_confirmation_token (account_id, confirmation_token_hash)
        VALUES (@accountId, @confirmationTokenHash)
        """;

    private readonly string _getSql =
        """
        SELECT account_id as accountId, confirmation_token_hash as confirmationTokenHash
        FROM pending_confirmation_token
        WHERE account_id = @accountId
        """;

    private readonly string _deleteSql =
        """
        DELETE FROM pending_confirmation_token
        WHERE account_id = @accountId
        """;
}