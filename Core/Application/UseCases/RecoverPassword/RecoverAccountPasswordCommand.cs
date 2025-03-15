using FluentResults;
using MediatR;

namespace Core.Application.UseCases.RecoverPassword;

public record RecoverAccountPasswordCommand(string Email) : IRequest<Result>;