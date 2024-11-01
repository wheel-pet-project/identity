using Application.Application.Interfaces;
using Application.Errors;
using Application.Infrastructure.Interfaces.Repositories;
using Domain.AccountAggregate;
using FluentResults;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace Application.Application.UseCases.Account.ConfirmEmail;

public class ConfirmAccountEmailUseCase(IAccountRepository accountRepository) 
    : IUseCase<ConfirmAccountEmailRequest, ConfirmAccountEmailResponse>
{
    public async Task<Result<ConfirmAccountEmailResponse>> Execute(ConfirmAccountEmailRequest request)
    {
        var deleteConfirmationRecordResult = await accountRepository.DeleteConfirmationToken(
            request.AccountId, request.ConfirmationId);
        
        if (deleteConfirmationRecordResult.IsFailed)
            return Result.Fail(deleteConfirmationRecordResult.Errors);
        
        var gettingAccountResult = await accountRepository.GetById(request.AccountId);
        if (gettingAccountResult.HasError(e => e is NotFound))
            throw new ApplicationException("Failed to find account", 
                "Account that couldn't be found has been confirmed");
        
        var account = gettingAccountResult.Value;
        account.SetStatus(Status.PendingApproval);
        var updatingStatusResult = await accountRepository.UpdateStatus(account);
        
        return updatingStatusResult.IsSuccess
            ? Result.Ok(new ConfirmAccountEmailResponse())
            : Result.Fail(updatingStatusResult.Errors);
    }
}