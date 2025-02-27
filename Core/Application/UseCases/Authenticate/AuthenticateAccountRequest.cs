using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authenticate;

public record AuthenticateAccountRequest(
    string Email,
    string Password) : IRequest<Result<AuthenticateAccountResponse>>;