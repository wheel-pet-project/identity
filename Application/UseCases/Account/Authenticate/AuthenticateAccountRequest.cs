namespace Application.UseCases.Account.Authenticate;

public class AuthenticateAccountRequest(string email, string password)
{
    public string Email { get; init; } = email;

    public string Password { get; init; } = password;
}