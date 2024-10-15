namespace Application.UseCases.Account.Authenticate;

public class AuthenticateAccountResponse(string accessToken, string refreshToken)
{
    public string AccessToken { get; init; } = accessToken;

    public string RefreshToken { get; init; } = refreshToken;
}