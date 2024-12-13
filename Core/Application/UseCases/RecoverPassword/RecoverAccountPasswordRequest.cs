using FluentResults;
using MediatR;

namespace Core.Application.UseCases.RecoverPassword;

public record RecoverAccountPasswordRequest(
    Guid CorrelationId,
    string Email) : BaseRequest(CorrelationId), IRequest<Result>;