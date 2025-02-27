using FluentResults;
using MediatR;

namespace Core.Application.UseCases.RefreshAccessToken;

public record RefreshAccountAccessTokenRequest(string RefreshToken)
    : IRequest<Result<RefreshAccountAccessTokenResponse>>;