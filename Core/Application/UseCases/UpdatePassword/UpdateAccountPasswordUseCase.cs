using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using FluentResults;

namespace Core.Application.UseCases.UpdatePassword;

public class UpdateAccountPasswordUseCase(IAccountRepository accountRepository,
    IPasswordRecoverTokenRepository passwordRecoverTokenRepository,
    IHasher hasher) 
    : IUseCase<UpdateAccountPasswordRequest, UpdateAccountPasswordResponse>
{
    public async Task<Result<UpdateAccountPasswordResponse>> Execute(
        UpdateAccountPasswordRequest request)
    {
        var gettingAccountResult = await accountRepository.GetByEmail(request.Email);
        if (gettingAccountResult.IsFailed) return Result.Fail(gettingAccountResult.Errors);
        var account = gettingAccountResult.Value;

        var gettingResetPasswordTokenResult = await passwordRecoverTokenRepository.GetPasswordRecoverToken(
            account.Id);
        if (gettingResetPasswordTokenResult.IsFailed) return Result.Fail(gettingResetPasswordTokenResult.Errors);
        var resetToken = gettingResetPasswordTokenResult.Value;
        
        if (resetToken.IsAlreadyApplied && resetToken.ExpiresIn < DateTime.UtcNow)
            return Result.Fail("Password reset token has expired or already applied");
        if (!hasher.VerifyHash(request.ResetToken.ToString(), resetToken.ResetPasswordTokenHash)) 
            return Result.Fail("Invalid reset password token");
        
        account.SetPasswordHash(hasher.GenerateHash(request.NewPassword));
        var updatingPasswordResult = await accountRepository.UpdatePassword(account);
        if (updatingPasswordResult.IsFailed) return Result.Fail(updatingPasswordResult.Errors);

        var markingAsAppliedResetTokenResult = await passwordRecoverTokenRepository
            .MarkPasswordRecoverTokenAsApplied(account.Id);
        
        return markingAsAppliedResetTokenResult.IsSuccess
            ? Result.Ok()
            : Result.Fail(updatingPasswordResult.Errors);
    }
}