using EnterpriseEmployeeManagement.Domain.Common;

namespace EnterpriseEmployeeManagement.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public decimal Salary { get; set; }

        // Foreign Key to Department
        public int DepartmentId { get; set; }

        public Department Department { get; set; } = null!;

        // Foreign Key to Role
        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}