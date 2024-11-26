namespace Core.Application.UseCases.RefreshAccessToken;

public record RefreshAccountAccessTokenResponse(
    string AccessToken, 
    string RefreshToken);