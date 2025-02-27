using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authorize;

public record AuthorizeAccountRequest(string AccessToken) : IRequest<Result<AuthorizeAccountResponse>>;