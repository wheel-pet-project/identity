namespace Application.Application.UseCases.Account.RefreshAccessToken;

public record RefreshAccountAccessTokenResponse(
    string AccessToken, 
    string RefreshToken);