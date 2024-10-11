namespace Application.UseCases.Account.Authenticate;

public class AuthenticateAccountResponse(string accessToken)
{
    public string AccessToken { get; init; } = accessToken;
}