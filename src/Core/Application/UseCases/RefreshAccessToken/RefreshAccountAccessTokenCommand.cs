using FluentResults;
using MediatR;

namespace Core.Application.UseCases.RefreshAccessToken;

public record RefreshAccountAccessTokenCommand(string RefreshToken)
    : IRequest<Result<RefreshAccountAccessTokenResponse>>;