using System.Data;
using System.Diagnostics;
using Application.Errors;
using Application.Infrastructure.Interfaces.Repositories;
using Dapper;
using Domain.AccountAggregate;
using FluentResults;
using Infrastructure.Repositories.Implementations.DapperModels;

namespace Infrastructure.Repositories.Implementations;

public class AccountRepository(IDbConnection connection) : IAccountRepository
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
        
        var queryCommand = new CommandDefinition(sql, new { accountId }, 
            cancellationToken: cancellationToken);
        
        var accounts = await connection
            .QueryAsync<Account, Role, Status, Account>(queryCommand,
                (account, role, status) =>
                {
                    account.SetRole(role);
                    account.SetStatus(status);

                    return account;
                }, 
                "role_id, status_id");
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
        
        var queryCommand = new CommandDefinition(sql, new { email = email }, 
            cancellationToken: cancellationToken);

        var accounts = await connection
            .QueryAsync<Account, Role, Status, Account>(queryCommand,
                (account, role, status) =>
                {
                    account.SetRole(role);
                    account.SetStatus(status);

                    return account;
                }, 
                "role_id, status_id");
        var accountList = accounts.ToList();
        
        return (accountList.Count != 0
            ? Result.Ok(accountList.FirstOrDefault())!
            : Result.Fail(new NotFound("Account not found")))!;
    }

    public async Task<Result<string>> GetConfirmationToken(Guid accountId)
    {
        var sql = @"
SELECT confirmation_token
FROM pending_confirmation_token
WHERE account_id = @accountId
LIMIT 1";

        var command = new CommandDefinition(sql, new { accountId });
        var result = await connection.QuerySingleOrDefaultAsync<string>(command);
        
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
        var affectedRows = await connection.ExecuteAsync(command);
        
        return affectedRows > 0
            ? Result.Ok() 
            : Result.Fail(new DbError("Failed to delete confirmation token in db"));
    }

    public async Task<Result> AddRefreshTokenInfo(Guid refreshTokenId, Guid accountId)
    {
        var sql = @"
INSERT INTO refresh_token_info (id, account_id, is_revoked, issue_date)
VALUES (@refreshTokenId, @accountId, false, @issueDate);";
        
        var command = new CommandDefinition(sql, new
        {
            accountId, refreshTokenId, issueDate = DateTime.UtcNow
        });
        var affectedRows = await connection.ExecuteAsync(command);
        
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
        var result = await connection
            .QuerySingleOrDefaultAsync<RefreshTokenInfoModel>(command);

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

        var affectedRows = await connection.ExecuteAsync(command);

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
        
        return isSuccess 
            ? Result.Ok()
            : Result.Fail(new DbError("Update refresh token in db failed"));
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
        
        connection.Open();
        using var transaction = connection.BeginTransaction();
        
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
        
        return isSuccess
            ? Result.Ok()
            : Result.Fail(new DbError("Adding account to db failed"));
    }

    public async Task<Result> UpdateStatus(Account account)
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
        
        return affectedRows > 0
            ? Result.Ok()
            : Result.Fail(new DbError("Failed to update account status in db"));
    }

    public async Task<Result> AddResetPasswordToken(Guid accountId, Guid resetToken, DateTime expiresIn)
    {
        var sql = @"
INSERT INTO password_reset_token (account_id, reset_token, is_already_applied, expires_in)
VALUES (@accountId, @resetToken, false, @expiresIn);";
        
        var command = new CommandDefinition(sql, new { accountId, resetToken, expiresIn });
        var affectedRows = await connection.ExecuteAsync(command);
        
        return affectedRows > 0
            ? Result.Ok()
            : Result.Fail(new DbError("Failed to add password reset token in db"));
    }

    public async Task<Result<(string ResetPasswordTokenHash, 
        bool IsAlreadyApplied,
        DateTime ExpiresIn)>> GetResetPasswordToken(Guid accountId)
    {
        var sql = @"
SELECT reset_token, is_already_applied, expires_in
FROM password_reset_token
WHERE account_id = @accountId
LIMIT 1";
        
        var command = new CommandDefinition(sql, new { accountId });

        var result = await connection.QuerySingleOrDefaultAsync<PasswordResetTokenModel>(command);
        
        return result is not null
            ? Result.Ok((result.reset_token, result.is_already_applied, result.expires_in))
            : Result.Fail(new NotFound("Password reset token not found"));
    }

    public async Task<Result> MarkResetPasswordAsApplied(Guid accountId)
    {
        var sql = @"
UPDATE password_reset_token
SET is_already_applied = true
WHERE account_id = @accountId";
        
        var command = new CommandDefinition(sql, new { accountId });
        
        var affectedRows = await connection.ExecuteAsync(command);
        
        return affectedRows > 0
            ? Result.Ok()
            : Result.Fail(new DbError("Failed to update password reset token's status in db"));
    }

    public async Task<Result> UpdatePassword(Account account)
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
        
        return affectedRows > 0 
            ? Result.Ok() 
            : Result.Fail(new DbError("Failed to update password in db"));
    }
}