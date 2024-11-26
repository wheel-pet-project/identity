namespace Core.Application.UseCases.Authenticate;

public record AuthenticateAccountResponse(
    string AccessToken, 
    string RefreshToken);