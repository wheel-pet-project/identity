namespace Core.Application.UseCases.ConfirmEmail;

public record ConfirmAccountEmailRequest(
    Guid AccountId, 
    Guid ConfirmationToken);