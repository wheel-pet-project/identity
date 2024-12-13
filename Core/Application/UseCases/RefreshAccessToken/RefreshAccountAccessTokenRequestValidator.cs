using FluentValidation;

namespace Core.Application.UseCases.RefreshAccessToken;

public class RefreshAccountAccessTokenRequestValidator : AbstractValidator<RefreshAccountAccessTokenRequest>
{
    public RefreshAccountAccessTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Refresh token is required");
    }
}