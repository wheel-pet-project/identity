using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Ports.Postgres;
using FluentResults;

namespace Application.Application.UseCases.Account.RecoverPassword;

public class RecoverAccountPasswordUseCase(IAccountRepository accountRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IHasher hasher)
    : IUseCase<RecoverAccountPasswordRequest, RecoverAccountPasswordResponse>
{
    public async Task<Result<RecoverAccountPasswordResponse>> Execute(
        RecoverAccountPasswordRequest request)
    {
        var gettingAccountResult = await accountRepository.GetByEmail(request.Email);
        if (gettingAccountResult.IsFailed)
            return Result.Fail(gettingAccountResult.Errors);
        var account = gettingAccountResult.Value;

        var resetToken = Guid.NewGuid();
        var resetTokenHash = hasher.GenerateHash(resetToken.ToString());
        var addingResetPasswordToken = await passwordResetTokenRepository.AddPasswordResetToken(
            account.Id,
            resetTokenHash,
            DateTime.UtcNow.AddMinutes(10));

        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine(resetToken);
            Console.BackgroundColor = default;
        }
        // send token in notification
        
        return addingResetPasswordToken.IsSuccess
            ? Result.Ok()
            : Result.Fail(addingResetPasswordToken.Errors);
    }
}