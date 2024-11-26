using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Errors;
using Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using FluentResults;

namespace Core.Application.UseCases.ConfirmEmail;

public class ConfirmAccountEmailUseCase(IAccountRepository accountRepository, 
    IHasher hasher) 
    : IUseCase<ConfirmAccountEmailRequest, ConfirmAccountEmailResponse>
{
    public async Task<Result<ConfirmAccountEmailResponse>> Execute(ConfirmAccountEmailRequest request)
    {
        var gettingConfirmationTokenResult = await accountRepository.GetConfirmationToken(request.AccountId);
        if (gettingConfirmationTokenResult.IsFailed) 
            return Result.Fail(gettingConfirmationTokenResult.Errors);
        var tokenHash = gettingConfirmationTokenResult.Value;
        
        if (!hasher.VerifyHash(request.ConfirmationToken.ToString(), tokenHash)) 
            return Result.Fail("Invalid Confirmation token");
        
        var deletingConfirmationTokenResult = await accountRepository
            .DeleteConfirmationToken(request.AccountId, request.ConfirmationToken);
        if (deletingConfirmationTokenResult.IsFailed) return Result.Fail(deletingConfirmationTokenResult.Errors);
        var gettingAccountResult = await accountRepository.GetById(request.AccountId);
        
        if (gettingAccountResult.HasError(e => e is NotFound)) throw new DataConsistencyViolationException( 
                "Data consistency violation: account that couldn't be found has been confirmed");
        var account = gettingAccountResult.Value;
        account.SetStatus(Status.PendingApproval);
        var updatingStatusResult = await accountRepository.UpdateStatus(account);
        
        return updatingStatusResult.IsSuccess
            ? Result.Ok(new ConfirmAccountEmailResponse())
            : Result.Fail(updatingStatusResult.Errors);
    }
}