using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using FluentValidation;

namespace EnterpriseEmployeeManagement.Application.Validators
{
    public class LoginValidator : AbstractValidator<LoginRequest>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}