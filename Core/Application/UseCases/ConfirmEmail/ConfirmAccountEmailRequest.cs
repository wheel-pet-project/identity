using FluentResults;
using MediatR;

namespace Core.Application.UseCases.ConfirmEmail;

public record ConfirmAccountEmailRequest(
    Guid CorrelationId,
    Guid AccountId, 
    Guid ConfirmationToken) 
    : BaseRequest(CorrelationId), IRequest<Result>;