using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;
using Core.Infrastructure.Interfaces.PasswordHasher;
using FluentResults;

namespace Core.Domain.Services.UpdateAccountPasswordService;

public class UpdateAccountPasswordService(IHasher hasher) : IUpdateAccountPasswordService
{
    public Result UpdatePassword(Account account, string potentialPassword)
    {
        if (Account.ValidateNotHashedPassword(potentialPassword) == false)
            throw new ValueOutOfRangeException(
                $"{nameof(potentialPassword)} must be at between 6 and 30 characters long");

        var passwordHash = hasher.GenerateHash(potentialPassword);
        
        account.SetPasswordHash(passwordHash);
        
        return Result.Ok();
    }
}