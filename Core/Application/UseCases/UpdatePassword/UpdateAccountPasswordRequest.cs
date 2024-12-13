using FluentResults;
using MediatR;

namespace Core.Application.UseCases.UpdatePassword;

public record UpdateAccountPasswordRequest(
    Guid CorrelationId,
    string NewPassword,
    string Email,
    Guid RecoverToken) : BaseRequest(CorrelationId), IRequest<Result>;