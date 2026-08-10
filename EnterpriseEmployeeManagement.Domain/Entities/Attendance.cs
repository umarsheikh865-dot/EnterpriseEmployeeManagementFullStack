using EnterpriseEmployeeManagement.Domain.Common;

namespace EnterpriseEmployeeManagement.Domain.Entities
{
    public class Attendance : BaseEntity
    {
        public DateTime Date { get; set; }
        public string Status { get; set; } = string.Empty;

        // Foreign Key
        public int EmployeeId { get; set; }

        // Navigation Property
        public Employee Employee { get; set; } = null!;
    }
}