namespace Application.UseCases.Account.Authorize;

public class AuthorizeAccountRequest(string accessToken)
{
    public string AccessToken { get; init; } = accessToken;
}