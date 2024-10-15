using Application.Application.Interfaces;
using Application.Infrastructure.Interfaces.Repositories;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace Application.UseCases.Account.ConfirmEmail;

public class ConfirmEmailUseCase(IAccountRepository accountRepository) 
    : IUseCase<ConfirmEmailRequest, ConfirmEmailResponse>
{
    public async Task<ConfirmEmailResponse> Execute(ConfirmEmailRequest request)
    {
        var confirmationRecord = await accountRepository
            .GetConfirmationRecord(request.AccountId);

        if (confirmationRecord.isExist)
        {
            if (request.ConfirmationId == confirmationRecord.confirmationId)
                return new ConfirmEmailResponse();

            throw new ApplicationException("Confirmation failed",
                $"Confirm email failed because confirmationId don't match for account with id: {request.AccountId}");
        }
        
        throw new ApplicationException("Confirmation failed",
            $"Confirm email record not found for account with id: {request.AccountId}");
    }
}