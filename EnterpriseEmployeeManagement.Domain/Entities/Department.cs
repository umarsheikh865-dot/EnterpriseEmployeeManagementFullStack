using EnterpriseEmployeeManagement.Domain.Common;

namespace EnterpriseEmployeeManagement.Domain.Entities
{
    public class Department : BaseEntity
    {
        // Department Name
        public string Name { get; set; } = string.Empty;

        // Department Description
        public string Description { get; set; } = string.Empty;

        // One Department can have many Employees
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}