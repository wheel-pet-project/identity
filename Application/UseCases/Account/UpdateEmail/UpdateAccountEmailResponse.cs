namespace Application.UseCases.Account.UpdateEmail;

public class UpdateAccountEmailResponse(string updatedEmail)
{
    public string UpdatedEmail { get; init; } = updatedEmail;
}