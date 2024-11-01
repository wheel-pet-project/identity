namespace Application.Application.UseCases.Account.Authenticate;

public record AuthenticateAccountResponse(
    string AccessToken, 
    string RefreshToken);