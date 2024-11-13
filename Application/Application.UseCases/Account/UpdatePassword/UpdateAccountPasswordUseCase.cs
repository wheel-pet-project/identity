using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Ports.Postgres;
using FluentResults;

namespace Application.Application.UseCases.Account.UpdatePassword;

public class UpdateAccountPasswordUseCase(IAccountRepository accountRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IHasher hasher) 
    : IUseCase<UpdateAccountPasswordRequest, UpdateAccountPasswordResponse>
{
    public async Task<Result<UpdateAccountPasswordResponse>> Execute(
        UpdateAccountPasswordRequest request)
    {
        var gettingAccountResult = await accountRepository.GetByEmail(request.Email);
        if (gettingAccountResult.IsFailed)
            return Result.Fail(gettingAccountResult.Errors);
        var account = gettingAccountResult.Value;

        var gettingResetPasswordTokenResult = await passwordResetTokenRepository
            .GetPasswordResetToken(account.Id);
        if (gettingResetPasswordTokenResult.IsFailed)
            return Result.Fail(gettingResetPasswordTokenResult.Errors);
        var resetToken = gettingResetPasswordTokenResult.Value;
        
        if (resetToken.IsAlreadyApplied && resetToken.ExpiresIn < DateTime.UtcNow)
            return Result.Fail("Password reset token has expired or already applied");
        if (!hasher.VerifyHash(request.ResetToken.ToString(), resetToken.ResetPasswordTokenHash))
            return Result.Fail("Invalid reset password token");
        
        account.SetPassword(hasher.GenerateHash(request.NewPassword));
        var updatingPasswordResult = await accountRepository.UpdatePassword(account);
        if (updatingPasswordResult.IsFailed)
            return Result.Fail(updatingPasswordResult.Errors);

        var markingAsAppliedResetTokenResult = await passwordResetTokenRepository
            .MarkPasswordResetTokenAsApplied(account.Id);
        
        return markingAsAppliedResetTokenResult.IsSuccess
            ? Result.Ok()
            : Result.Fail(updatingPasswordResult.Errors);
    }
}