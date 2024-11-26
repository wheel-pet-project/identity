namespace Core.Application.UseCases.Authenticate;

public record AuthenticateAccountRequest(
    string Email, 
    string Password);