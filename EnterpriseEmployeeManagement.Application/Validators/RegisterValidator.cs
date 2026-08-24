using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using FluentValidation;

namespace EnterpriseEmployeeManagement.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("First name is required.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Last name is required.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters.");
        }
    }
}