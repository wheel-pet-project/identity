using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;

namespace Application.UseCases.Account.UpdatePassword;

public class UpdateAccountPasswordUseCase(
    IAccountRepository accountRepository,
    IHasher hasher) 
    : IUseCase<UpdateAccountPasswordRequest, UpdateAccountPasswordResponse>
{
    public Task<UpdateAccountPasswordResponse> Execute(
        UpdateAccountPasswordRequest request)
    {
        throw new NotImplementedException();
    }
}