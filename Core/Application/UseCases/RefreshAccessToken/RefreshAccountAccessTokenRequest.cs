using FluentResults;
using MediatR;

namespace Core.Application.UseCases.RefreshAccessToken;

public record RefreshAccountAccessTokenRequest(
    Guid CorrelationId,
    string RefreshToken) 
    : BaseRequest(CorrelationId), IRequest<Result<RefreshAccountAccessTokenResponse>>;