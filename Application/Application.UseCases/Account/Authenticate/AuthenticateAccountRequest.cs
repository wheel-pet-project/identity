namespace Application.Application.UseCases.Account.Authenticate;

public record AuthenticateAccountRequest(
    string Email, 
    string Password);