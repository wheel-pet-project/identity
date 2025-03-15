using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authenticate;

public record AuthenticateAccountCommand(
    string Email,
    string Password) : IRequest<Result<AuthenticateAccountResponse>>;