using Core.Domain.AccountAggregate;
using Core.Ports.Postgres.Repositories;
using Dapper;
using Infrastructure.Settings;

namespace Infrastructure.Adapters.Postgres.Repositories;

public class AccountRepository(
    DbSession session, 
    PostgresRetryPolicy retryPolicy) : IAccountRepository
{
    public async Task<Account?> GetById(Guid accountId, CancellationToken cancellationToken = default)
    {
        var sql = @"
SELECT account.id, account.email, account.phone, account.password_hash as passwordHash, 
       account.role_id, account.status_id, role.id, role.name, status.id, status.name
FROM account 
INNER JOIN role ON account.role_id = role.id
INNER JOIN status ON account.status_id = status.id
WHERE account.id = @accountId
LIMIT 1";

        var queryCommand = new CommandDefinition(sql, new { accountId }, cancellationToken: cancellationToken,
             transaction: session.Transaction);

        var accounts = await retryPolicy.ExecuteAsync(async () =>
        {
            return await session.Connection.QueryAsync<Account, Role, Status, Account>(queryCommand,
                (account, role, status) =>
                {
                    var roleField = account.GetType().GetProperty("Role");
                    var statusField = account.GetType().GetProperty("Status");

                    roleField!.SetValue(account, Role.FromId(role.Id));
                    statusField!.SetValue(account, Status.FromId(status.Id));

                    return account;
                });
        });
        
        var accountList = accounts.ToList();
        return accountList.AsList().FirstOrDefault();
    }

    public async Task<Account?> GetByEmail(
        string email, CancellationToken cancellationToken = default)
    {
        var sql = @"
SELECT account.id, account.email, account.phone, account.password_hash as passwordHash, 
       account.role_id, account.status_id, role.id, role.name, status.id, status.name 
FROM account 
INNER JOIN role ON account.role_id = role.id
INNER JOIN status ON account.status_id = status.id
WHERE email = @email
LIMIT 1";

        var queryCommand = new CommandDefinition(sql, new { email }, cancellationToken: cancellationToken,
            transaction: session.Transaction);

        var accounts = await retryPolicy.ExecuteAsync(async () =>
        {
            return await session.Connection.QueryAsync<Account, Role, Status, Account>(queryCommand,
                (account, role, status) =>
                {
                    var roleField = account.GetType().GetProperty("Role");
                    var statusField = account.GetType().GetProperty("Status");

                    roleField!.SetValue(account, Role.FromId(role.Id));
                    statusField!.SetValue(account, Status.FromId(status.Id));

                    return account;
                });
        });
        
        var accountList = accounts.ToList();
        return accountList.AsList().FirstOrDefault();
    }

    public async Task Add(Account account)
    {
        var sqlForCreateAccount = @"
INSERT INTO account (id, role_id, status_id, email, phone, password_hash)
VALUES (@id, @roleId, @statusId, @email, @phone, @passwordHash)";

        
        var createCommand = new CommandDefinition(sqlForCreateAccount,
            new
            {
                id = account.Id,
                roleId = account.Role.Id,
                statusId = account.Status.Id,
                account.Email,
                account.Phone,
                account.PasswordHash,
            }, session.Transaction);
        
        await session.Connection.ExecuteAsync(createCommand);
    }

    public async Task UpdatePasswordHash(Account account)
    {
        var sql = @"
UPDATE account 
SET password_hash = @passwordHash 
WHERE id = @id";

        await session.Connection.ExecuteAsync(sql, 
            new { id = account.Id, account.PasswordHash }, session.Transaction);
    }

    public async Task UpdateStatus(Account account)
    {
        var sql = @"
UPDATE account 
SET status_id = @statusId 
WHERE id = @id";

        await session.Connection.ExecuteAsync(sql, 
            new { id = account.Id, statusId = account.Status.Id }, session.Transaction);
    }
}