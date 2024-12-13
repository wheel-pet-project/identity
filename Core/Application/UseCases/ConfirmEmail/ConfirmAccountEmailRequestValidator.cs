using FluentValidation;

namespace Core.Application.UseCases.ConfirmEmail;

public class ConfirmAccountEmailRequestValidator : AbstractValidator<ConfirmAccountEmailRequest>
{
    public ConfirmAccountEmailRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("Account id invalid");
        RuleFor(x => x.ConfirmationToken).NotEmpty().WithMessage("Confirmation token invalid");
    }
}