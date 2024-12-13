using FluentResults;
using MediatR;

namespace Core.Application.UseCases.Authorize;

public record AuthorizeAccountRequest(
    Guid CorrelationId,
    string AccessToken) 
    : BaseRequest(CorrelationId), IRequest<Result<AuthorizeAccountResponse>>;