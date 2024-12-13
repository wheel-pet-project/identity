using FluentValidation;

namespace Core.Application.UseCases.Authorize;

public class AuthorizeAccountRequestValidator : AbstractValidator<AuthorizeAccountRequest>
{
    public AuthorizeAccountRequestValidator()
    {
        RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Access token is required");
    }
}