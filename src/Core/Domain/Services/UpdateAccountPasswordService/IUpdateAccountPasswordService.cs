using Core.Domain.AccountAggregate;
using FluentResults;

namespace Core.Domain.Services.UpdateAccountPasswordService;

public interface IUpdateAccountPasswordService
{
    Result UpdatePassword(Account account, string potentialPassword);
}