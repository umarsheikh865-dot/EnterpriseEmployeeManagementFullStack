using EnterpriseEmployeeManagement.Application.DTOs.Employees;
using FluentValidation;

namespace EnterpriseEmployeeManagement.Application.Validators
{
    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("First name is required and cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Last name is required and cannot exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Salary cannot be negative.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("DepartmentId must be greater than 0.");

            RuleFor(x => x.RoleId)
                .GreaterThan(0)
                .WithMessage("RoleId must be greater than 0.");
        }
    }
}