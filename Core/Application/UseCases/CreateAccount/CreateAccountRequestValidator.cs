using System.Text.RegularExpressions;
using FluentValidation;

namespace Core.Application.UseCases.CreateAccount;

public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .MinimumLength(10).WithMessage("Phone must not be less than 10 characters.")
            .MaximumLength(20).WithMessage("Phone must not be greater than 20 characters")
            .Matches(new Regex(@"(^8|7|\+7)((\d{10})|(\s\(\d{3}\)\s\d{3}\s\d{2}\s\d{2}))"))
            .WithMessage("Phone not valid");
        
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required");    
    }
}