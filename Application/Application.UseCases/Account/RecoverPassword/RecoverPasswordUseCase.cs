using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.Repositories;
using FluentResults;

namespace Application.Application.UseCases.Account.RecoverPassword;

public class RecoverPasswordUseCase(IAccountRepository accountRepository)
    : IUseCase<RecoverPasswordRequest, RecoverPasswordResponse>
{
    public Task<Result<RecoverPasswordResponse>> Execute(RecoverPasswordRequest request)
    {
        throw new NotImplementedException();
    }
}