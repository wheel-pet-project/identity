using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authorize;

public record AuthorizeAccountCommand(string AccessToken) : IRequest<Result<AuthorizeAccountResponse>>;