using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using FluentResults;

namespace Application.Application.UseCases.Account.UpdatePassword;

public class UpdateAccountPasswordUseCase() 
    : IUseCase<UpdateAccountPasswordRequest, UpdateAccountPasswordResponse>
{
    public Task<Result<UpdateAccountPasswordResponse>> Execute(
        UpdateAccountPasswordRequest request)
    {
        throw new NotImplementedException();
    }
}