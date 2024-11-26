using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using FluentResults;

namespace Core.Application.UseCases.RecoverPassword;

public class RecoverAccountPasswordUseCase(IAccountRepository accountRepository,
    IPasswordRecoverTokenRepository passwordRecoverTokenRepository,
    IHasher hasher)
    : IUseCase<RecoverAccountPasswordRequest, RecoverAccountPasswordResponse>
{
    public async Task<Result<RecoverAccountPasswordResponse>> Execute(
        RecoverAccountPasswordRequest request)
    {
        var gettingAccountResult = await accountRepository.GetByEmail(request.Email);
        if (gettingAccountResult.IsFailed) return Result.Fail(gettingAccountResult.Errors);
        var account = gettingAccountResult.Value;

        var resetToken = Guid.NewGuid();
        var resetTokenHash = hasher.GenerateHash(resetToken.ToString());
        var addingResetPasswordToken = await passwordRecoverTokenRepository.AddPasswordRecoverToken(
            account.Id, resetTokenHash, DateTime.UtcNow.AddMinutes(10));

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