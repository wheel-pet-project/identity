using FluentResults;
using MediatR;

namespace Core.Application.UseCases.RecoverPassword;

public record RecoverAccountPasswordRequest(string Email) : IRequest<Result>;