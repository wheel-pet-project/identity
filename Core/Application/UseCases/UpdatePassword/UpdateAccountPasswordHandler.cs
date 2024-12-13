using Core.Domain.SharedKernel.Errors;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.UpdatePassword;

public class UpdateAccountPasswordHandler(
    IPasswordRecoverTokenRepository passwordRecoverTokenRepository,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IHasher hasher) 
    : IRequestHandler<UpdateAccountPasswordRequest, Result>
{
    public async Task<Result> Handle(UpdateAccountPasswordRequest request, CancellationToken _)
    {
        var account = await accountRepository.GetByEmail(request.Email);
        if (account == null) return Result.Fail(new NotFound("Account not found"));
        
        var passwordRecoverToken = await passwordRecoverTokenRepository.Get(account.Id);
        if (passwordRecoverToken == null) return Result.Fail(new NotFound("Password recover token not found"));
        
        if (!passwordRecoverToken.IsValid()) return Result.Fail("Password reset token has expired or already applied");
        if (!hasher.VerifyHash(request.RecoverToken.ToString(), passwordRecoverToken.RecoverTokenHash)) 
            return Result.Fail("Invalid reset password token");
        
        passwordRecoverToken.Apply();
        account.SetPasswordHash(hasher.GenerateHash(request.NewPassword));
        
        await unitOfWork.BeginTransaction();
        await accountRepository.UpdatePasswordHash(account);
        await passwordRecoverTokenRepository.UpdateAppliedStatus(passwordRecoverToken);
        
        return await unitOfWork.Commit();
    }
}