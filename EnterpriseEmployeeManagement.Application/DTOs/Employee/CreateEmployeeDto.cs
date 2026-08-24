using System.ComponentModel.DataAnnotations;

namespace EnterpriseEmployeeManagement.Application.DTOs.Employees
{
    public class CreateEmployeeDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Salary { get; set; }

        [Range(1, int.MaxValue)]
        public int DepartmentId { get; set; }

        [Range(1, int.MaxValue)]
        public int RoleId { get; set; }
    }
}