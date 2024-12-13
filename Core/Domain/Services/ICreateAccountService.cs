using Core.Domain.AccountAggregate;
using FluentResults;

namespace Core.Domain.Services;

public interface ICreateAccountService
{
    Task<Result<Account>> CreateUser(Role role, string email, string phone, string passwordHash);
}