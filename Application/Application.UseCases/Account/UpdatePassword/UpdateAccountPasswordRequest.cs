namespace Application.Application.UseCases.Account.UpdatePassword;

public record UpdateAccountPasswordRequest(
    string NewPassword,
    string Email,
    Guid ResetToken);