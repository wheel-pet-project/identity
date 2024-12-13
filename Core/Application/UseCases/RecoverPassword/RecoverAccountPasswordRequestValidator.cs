using FluentValidation;

namespace Core.Application.UseCases.RecoverPassword;

public class RecoverAccountPasswordRequestValidator : AbstractValidator<RecoverAccountPasswordRequest>
{
    public RecoverAccountPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is invalid.");
    }
}