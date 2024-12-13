using FluentValidation;

namespace Core.Application.UseCases.UpdatePassword;

public class UpdateAccountPasswordRequestValidator : AbstractValidator<UpdateAccountPasswordRequest>
{
    public UpdateAccountPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid");

        RuleFor(x => x.RecoverToken)
            .NotEmpty().WithMessage("Recover token cannot be empty uuid");
        
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters");
    }
}