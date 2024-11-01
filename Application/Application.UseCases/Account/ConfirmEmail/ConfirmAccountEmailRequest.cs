namespace Application.Application.UseCases.Account.ConfirmEmail;

public record ConfirmAccountEmailRequest(
    Guid AccountId, 
    Guid ConfirmationId);