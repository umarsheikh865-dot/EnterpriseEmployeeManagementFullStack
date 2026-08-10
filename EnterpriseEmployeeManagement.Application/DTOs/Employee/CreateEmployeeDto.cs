namespace EnterpriseEmployeeManagement.Application.DTOs.Employees
{
    public class CreateEmployeeDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        public int DepartmentId { get; set; }

        public int RoleId { get; set; }
    }
}