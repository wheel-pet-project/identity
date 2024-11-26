using System.Data;
using System.Diagnostics;
using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Errors;
using Core.Ports.Postgres;
using Dapper;
using FluentResults;
using Infrastructure.Settings;

namespace Infrastructure.Adapters.Postgres;

public class AccountRepository(IDbConnection connection, PostgresRetryPolicy retryPolicy) 
    : IAccountRepository
{
    private readonly ActivitySource _activitySource = new("Identity");

    public async Task<Result<Account>> GetById(
        Guid accountId, CancellationToken cancellationToken = default)
    {
        var sql = @"
SELECT * 
FROM account 
INNER JOIN role ON account.role_id = role.Id
INNER JOIN status ON account.status_id = status.Id
WHERE account.id = @accountId
LIMIT 1";
        
        var queryCommand = new CommandDefinition(sql, new { accountId }, cancellationToken: cancellationToken);

        var accounts = await retryPolicy.ExecuteAsync(() =>
            connection.QueryAsync<Account, Role, Status, Account>(queryCommand,
                (account, role, status) =>
                {
                    account.SetRole(role);
                    account.SetStatus(status);

                    return account;
                },
                "role_id, status_id"));
        var accountList = accounts.ToList();
        
        return (accountList.Count != 0
            ? Result.Ok(accountList.FirstOrDefault())!
            : Result.Fail(new NotFound("Account not found")))!;
    }

    public async Task<Result<Account>> GetByEmail(
        string email, CancellationToken cancellationToken = default)
    {
        var sql = @"
SELECT * 
FROM account 
INNER JOIN role ON account.role_id = role.Id
INNER JOIN status ON account.status_id = status.Id
WHERE email = @email
LIMIT 1";
        
        var queryCommand = new CommandDefinition(sql, new { email }, cancellationToken: cancellationToken);

        var accounts = await retryPolicy.ExecuteAsync(() => 
            connection.QueryAsync<Account, Role, Status, Account>(queryCommand, 
                (account, role, status) =>
                {
                    account.SetRole(role);
                    account.SetStatus(status);

                    return account;
                }, 
                "role_id, status_id"));
        var accountList = accounts.ToList();
        
        return (accountList.Count != 0
            ? Result.Ok(accountList.FirstOrDefault())!
            : Result.Fail(new NotFound("Account not found")))!;
    }
    
    public async Task<Result> AddAccountAndConfirmationToken(Account account, string confirmationTokenHash)
    {
        using var activity = _activitySource.StartActivity("Adding account to db");
        activity?.SetTag("accountId", account.Id);

        var isSuccess = true;
        
        var sqlForCreateAccount = @"
INSERT INTO account (id, role_id, email, phone, password, status_id)
VALUES (@id, @roleId, @email, @phone, @password, @statusId)";
        
        var sqlForCreateConfirmRecord = @"
INSERT INTO pending_confirmation_token (account_id, confirmation_token)
VALUES (@accountId, @confirmationTokenHash)";

        await retryPolicy.ExecuteAsync(async () =>
        {
            connection.Open();
            using var transaction = connection.BeginTransaction();
        
            var createCommand = new CommandDefinition(sqlForCreateAccount,
                new
                {
                    id = account.Id,
                    roleId = account.Role.Id,
                    account.Email,
                    account.Phone,
                    Password = account.PasswordHash,
                    statusId = account.Status.Id
                }, transaction, 3);
            var createConfirmRecordCommand = new CommandDefinition(sqlForCreateConfirmRecord,
                new { accountId = account.Id, confirmationTokenHash }, transaction, 3);
        
            try
            {
                await connection.ExecuteAsync(createCommand);
                await connection.ExecuteAsync(createConfirmRecordCommand);
            
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
            : Result.Fail(new DbError("Adding account to db failed"));
    }
    
    public async Task<Result> UpdatePassword(Account account)
    {
        var sql = @"
UPDATE account 
SET password = @password 
WHERE id = @id";

        var affectedRows = await retryPolicy.ExecuteAsync(() => connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id, Password = account.PasswordHash
            }));
        
        return affectedRows > 0 
            ? Result.Ok() 
            : Result.Fail(new DbError("Failed to update password in db"));
    }
    
    public async Task<Result> UpdateStatus(Account account)
    {
        var sql = @"
UPDATE account 
SET status_id = @statusId 
WHERE id = @id";

        var affectedRows = await retryPolicy.ExecuteAsync(() => connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                statusId = account.Status.Id
            }));
        
        return affectedRows > 0
            ? Result.Ok()
            : Result.Fail(new DbError("Failed to update account status in db"));
    }

    public async Task<Result<string>> GetConfirmationToken(Guid accountId)
    {
        var sql = @"
SELECT confirmation_token
FROM pending_confirmation_token
WHERE account_id = @accountId
LIMIT 1";

        var command = new CommandDefinition(sql, new { accountId });
        var result = await retryPolicy.ExecuteAsync(() => connection.QuerySingleOrDefaultAsync<string>(command));
        
        return result is not null
            ? Result.Ok(result)
            : Result.Fail(new NotFound("Confirmation token not found"));
    }
    
    public async Task<Result> DeleteConfirmationToken(Guid accountId, Guid confirmationToken)
    {
        var sql = @"
DELETE FROM pending_confirmation_token
WHERE account_id = @accountId";
        
        var command = new CommandDefinition(sql, new { accountId, confirmationToken });
        var affectedRows = await retryPolicy.ExecuteAsync(() => connection.ExecuteAsync(command));
        
        return affectedRows > 0
            ? Result.Ok() 
            : Result.Fail(new DbError("Failed to delete confirmation token in db"));
    }
}