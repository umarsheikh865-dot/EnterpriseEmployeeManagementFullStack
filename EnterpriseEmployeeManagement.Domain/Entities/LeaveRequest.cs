using EnterpriseEmployeeManagement.Domain.Common;

namespace EnterpriseEmployeeManagement.Domain.Entities
{
    public class LeaveRequest : BaseEntity
    {
        // Foreign Key
        public int EmployeeId { get; set; }

        // Navigation Property
        public Employee Employee { get; set; } = null!;

        // Leave Start Date
        public DateTime StartDate { get; set; }

        // Leave End Date
        public DateTime EndDate { get; set; }

        // Reason for Leave
        public string Reason { get; set; } = string.Empty;

        // Leave Status
        public bool IsApproved { get; set; }
    }
}