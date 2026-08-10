using EnterpriseEmployeeManagement.Domain.Common;

namespace EnterpriseEmployeeManagement.Domain.Entities
{
    public class Role : BaseEntity
    {
        // Role Name
        public string Name { get; set; } = string.Empty;

        // One Role can be assigned to many Employees
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}