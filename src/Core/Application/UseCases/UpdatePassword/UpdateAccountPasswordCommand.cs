using FluentResults;
using MediatR;

namespace Core.Application.UseCases.UpdatePassword;

public record UpdateAccountPasswordCommand(
    string NewPassword,
    string Email,
    Guid RecoverToken) : IRequest<Result>;