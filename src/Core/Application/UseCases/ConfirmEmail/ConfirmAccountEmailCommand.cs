using FluentResults;
using MediatR;

namespace Core.Application.UseCases.ConfirmEmail;

public record ConfirmAccountEmailCommand(
    Guid AccountId,
    Guid ConfirmationToken) : IRequest<Result>;