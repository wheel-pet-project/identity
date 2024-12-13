using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Errors;
using Core.Ports.Postgres.Repositories;
using FluentResults;

namespace Core.Domain.Services;

public class CreateAccountService(IAccountRepository accountRepository) : ICreateAccountService
{
    public async Task<Result<Account>> CreateUser(Role role, string email, string phone, string passwordHash)
    {
        var userExists = await accountRepository.GetByEmail(email);
        if (userExists is not null)
            return Result.Fail(new AlreadyExists("Email already exists"));
        
        var account = Account.Create(role, email, phone, passwordHash);
        
        return Result.Ok(account);
    }
}