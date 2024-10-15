using System.Data;
using System.Diagnostics;
using Application.Infrastructure.Interfaces.Repositories;
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
INNER JOIN role ON account.role_id = role.Id
INNER JOIN status ON account.status_id = status.Id
WHERE id = @id
LIMIT 1";
        
        var queryCommand = new CommandDefinition(sql, new { id }, 
            cancellationToken: cancellationToken);

        var isSuccess = await connection
            .QueryAsync<Account, Role, Status, Account>(queryCommand,
                (account, role, status) =>
                {
                    account.SetRole(role);
                    account.SetStatus(status);

                    return account;
                }, 
                "role_id, status_id");
        var account = isSuccess.FirstOrDefault();
        return account;
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

        var isSuccess = await connection
            .QueryAsync<Account, Role, Status, Account>(queryCommand,
                (account, role, status) =>
                {
                    account.SetRole(role);
                    account.SetStatus(status);

                    return account;
                }, 
                "role_id, status_id");
        var account = isSuccess.FirstOrDefault();
        return account;
    }
    
    public async Task<(bool isExist, Guid confirmationId)> GetConfirmationRecord(Guid accountId)
    {
        bool isExist = true;
        var sql = @"
SELECT confirmation_id
FROM pending_confirmation
WHERE account_id = @accountId";

        var command = new CommandDefinition(sql, new { accountId });
        
        var result = await connection.QuerySingleOrDefaultAsync<Guid>(command);
        // todo: refactor if
        if (result != Guid.Empty)
            return (isExist, result);
        
        return (false, Guid.Empty);
    }
    
    public async Task<bool> DeleteConfirmationRecord(Guid accountId)
    {
        var sql = @"
DELETE FROM pending_confirmation
WHERE account_id = @accountId";
        
        var command = new CommandDefinition(sql, new { accountId });
        var affectedRows = await connection.ExecuteAsync(command);

        var isSuccess = affectedRows > 0;
        return isSuccess;
    }

    public async Task<bool> CreateAccount(Account account, Guid confirmationId)
    {
        using var activity = _activitySource.StartActivity("Adding account to db");
        activity?.SetTag("accountId", account.Id);

        var isSuccess = true;
        
        var sqlForCreateAccount = @"
INSERT INTO account (id, role_id, email, phone, password, status_id)
VALUES (@id, @roleId, @email, @phone, @password, @statusId)";
        
        var sqlForCreateConfirmRecord = @"
INSERT INTO pending_confirmation (account_id, confirmation_id)
VALUES (@accountId, @confirmationId)";
        
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var createCommand = new CommandDefinition(sqlForCreateAccount,
                new
                {
                    id = account.Id,
                    roleId = account.Role.Id,
                    account.Email,
                    account.Phone,
                    account.Password,
                    statusId = account.Status.Id
                }, transaction, 3);
            await connection.ExecuteAsync(createCommand);

            var createConfirmRecordCommand = new CommandDefinition(sqlForCreateConfirmRecord,
                new
                {
                    accountId = account.Id,
                    confirmationId = confirmationId
                }, transaction, 3);
            await connection.ExecuteAsync(createConfirmRecordCommand);
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            isSuccess = false;
        }
        return isSuccess;
    }

    public async Task<bool> CreateRefreshToken(Guid accountId, string refreshToken, DateTime expiresIn)
    {
        using var activity = _activitySource.StartActivity("Adding refresh token to db");
        activity?.SetTag("accountId", accountId);

        var isSuccess = true;

        var sqlForDeleteUsefulToken = @"
DELETE FROM refresh_token WHERE account_id = @accountId";
        
        var sqlForCreate = @"
INSERT INTO refresh_token (account_id, refresh_token, expires_in)
VALUES (@accountId, @refreshToken, @expiresIn)";
        
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            var deleteCommand = new CommandDefinition(sqlForDeleteUsefulToken, 
                new { accountId },
                transaction);
            await connection.ExecuteAsync(deleteCommand);
        
            var createCommand = new CommandDefinition(sqlForCreate, 
                new { accountId, refreshToken, expiresIn }, 
                transaction);
            await connection.ExecuteAsync(createCommand);
            
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            isSuccess = false;
        }

        return isSuccess;
    }

    public async Task<bool> UpdateStatus(Account account)
    {
        var sql = @"
UPDATE account 
SET status_id = @statusId 
WHERE id = @id";

        var affectedRows = await connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                statusId = account.Status.Id
            });
        
        var isSuccess = affectedRows > 0;
        return isSuccess;
    }

    public async Task<bool> UpdatePassword(Account account)
    {
        var sql = @"
UPDATE account 
SET password = @password 
WHERE id = @id";

        var affectedRows = await connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                account.Password
            });

        var isSuccess = affectedRows > 0;
        return isSuccess;
    }

    public async Task<bool> UpdateEmail(Account account)
    {
        var sql = @"
UPDATE account 
SET email = @email 
WHERE id = @id";

        var affectedRows = await connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                account.Email
            });
        
        var isSuccess = affectedRows > 0;
        return isSuccess;
    }
}