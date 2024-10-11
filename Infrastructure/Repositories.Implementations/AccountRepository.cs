using System.Data;
using System.Diagnostics;
using Application.Infrastructure.Interfaces.Repositories;
using Ardalis.SmartEnum.Dapper;
using Dapper;
using Domain.AccountAggregate;

namespace Infrastructure.Repositories.Implementations;

public class AccountRepository(IDbConnection connection) : IAccountRepository
{
    private readonly ActivitySource _activitySource = new("Identity");

    public async Task<Account?> GetById(
        Guid id, CancellationToken cancellationToken = default)
    {
        var sql = @"
SELECT * 
FROM account 
WHERE id = @id
LIMIT 1";
        
        var queryCommand = new CommandDefinition(sql, new { id }, 
            cancellationToken: cancellationToken);
        var result = await connection.QuerySingleOrDefaultAsync<Account>(queryCommand);
        return result;
    }

    public async Task<Account?> GetByEmail(
        string email, CancellationToken cancellationToken = default)
    {
        var sql = @"
SELECT * 
FROM account 
INNER JOIN role ON account.role_id = role.Id
INNER JOIN status ON account.status_id = status.Id
WHERE email = @email
LIMIT 1";
        
        var queryCommand = new CommandDefinition(sql, new { email }, 
            cancellationToken: cancellationToken);

        var result = await connection
            .QueryAsync<Account, Role, Status, Account>(queryCommand,
                (account, role, status) =>
                {
                    account.SetRole(role);
                    account.SetStatus(status);

                    return account;
                }, 
                "role_id, status_id");
        var account = result.FirstOrDefault();
        return account;
    }

    public async Task Create(Account account, Guid confirmationId)
    {
        using var activity = _activitySource.StartActivity("Adding account to db");
        activity?.SetTag("accountId", account.Id);
        
        var sqlForCreateAccount = @"
INSERT INTO account (id, role_id, email, phone, password, status_id)
VALUES (@id, @roleId, @email, @phone, @password, @statusId)";
        
        var sqlForCreateConfirmRecord = @"
INSERT INTO pending_confirmation (acc_id, confirmation_id)
VALUES (@accountId, @confirmationId)";
        
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            await connection.ExecuteAsync(sqlForCreateAccount, 
                new
                {
                    id = account.Id,
                    roleId = account.Role.Id,
                    account.Email,
                    account.Phone,
                    account.Password,
                    statusId = account.Status.Id
                }, transaction, 3);
            
            await connection.ExecuteAsync(sqlForCreateConfirmRecord,
                new
                {
                    accountId = account.Id,
                    confirmationId = confirmationId
                }, transaction, 3);
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateStatus(Account account)
    {
        var sql = @"
UPDATE account 
SET status_id = @statusId 
WHERE id = @id";

        await connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                statusId = account.Status.Id
            });
    }

    public async Task UpdatePassword(Account account)
    {
        var sql = @"
UPDATE account 
SET password = @password 
WHERE id = @id";

        await connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                account.Password
            });
    }

    public async Task UpdateEmail(Account account)
    {
        var sql = @"
UPDATE account 
SET email = @email 
WHERE id = @id";

        await connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                account.Email
            });
    }
}