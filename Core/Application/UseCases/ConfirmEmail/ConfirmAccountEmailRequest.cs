using FluentResults;
using MediatR;

namespace Core.Application.UseCases.ConfirmEmail;

public record ConfirmAccountEmailRequest(
    Guid AccountId,
    Guid ConfirmationToken) : IRequest<Result>;