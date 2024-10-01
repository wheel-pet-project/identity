using Dapper;
using Domain.AccountAggregate;
using Infrastructure.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Infrastructure.Repositories.Implementations;

public class AccountRepository(IOptions<DbConnectionOptions> dbConnectionOptions)
{
    private readonly DbConnectionOptions _dbConnectionOptions = dbConnectionOptions.Value;

    public async Task<Account?> GetById(Guid id)
    {
        var sql = @"SELECT * 
FROM account 
WHERE id = @id
LIMIT 1";
        await using var connection = new SqlConnection(_dbConnectionOptions.ConnectionString);
        await connection.OpenAsync();

        var result = await connection.QuerySingleOrDefaultAsync<Account>(sql, 
            new { id });
        return result;
    }

    public async Task<Account?> GetByEmail(string email)
    {
        var sql = @"SELECT * 
FROM account 
WHERE email = @email
LIMIT 1";
        await using var connection = new SqlConnection(_dbConnectionOptions.ConnectionString);
        await connection.OpenAsync();

        var result = await connection.QuerySingleOrDefaultAsync<Account>(sql, 
            new { email });
        return result;
    }
    
    public async Task Create(Account account)
    {
        var sql = @"INSERT INTO account (id, role_id, email, phone, password, status_id)
VALUES (@id, @roleId, @email, @phone, @password, @statusId)";
        await using var connection = new SqlConnection(_dbConnectionOptions.ConnectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql, 
            new
            {
                id = account.Id,
                roleId = account.Role.Id,
                account.Email,
                account.Phone,
                account.Password,
                statusId = account.Status.Id
            });
    }

    public async Task UpdateStatus(Account account)
    {
        var sql = @"UPDATE account 
SET status_id = @statusId 
WHERE id = @id";
        await using var connection = new SqlConnection(_dbConnectionOptions.ConnectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                statusId = account.Status.Id
            });
    }

    public async Task UpdatePassword(Account account)
    {
        var sql = @"UPDATE account 
SET password = @password 
WHERE id = @id";
        await using var connection = new SqlConnection(_dbConnectionOptions.ConnectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                account.Password
            });
    }

    public async Task UpdateEmail(Account account)
    {
        var sql = @"UPDATE account 
SET email = @email 
WHERE id = @id";
        await using var connection = new SqlConnection(_dbConnectionOptions.ConnectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(sql, 
            new 
            {
                id = account.Id,
                account.Email
            });
    }
}