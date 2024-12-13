using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authenticate;

public record AuthenticateAccountRequest(
    Guid CorrelationId,
    string Email,
    string Password)
    : BaseRequest(CorrelationId), IRequest<Result<AuthenticateAccountResponse>>;