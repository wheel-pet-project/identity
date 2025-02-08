using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Errors;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres.Repositories;
using FluentResults;

namespace Core.Domain.Services.CreateAccountService;

public class CreateAccountService(
    IAccountRepository accountRepository, 
    IHasher hasher) 
    : ICreateAccountService
{
    public async Task<Result<Account>> CreateUser(
        Role role,
        string email,
        string phone,
        string password,
        Guid confirmationTokenGuid)
    {
        var existingAccount = await accountRepository.GetByEmail(email);
        if (existingAccount is not null)
            return Result.Fail(new AlreadyExists($"account with this {nameof(email)} already exists"));

        if (Account.ValidateNotHashedPassword(password) == false)
            throw new ValueOutOfRangeException($"{nameof(password)} must be at between 6 and 30 characters long");
        
        var passwordHash = hasher.GenerateHash(password);
        
        var account = Account.Create(role, email, phone, passwordHash, confirmationTokenGuid);
        
        return Result.Ok(account);
    }
}