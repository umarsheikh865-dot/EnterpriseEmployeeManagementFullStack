namespace EnterpriseEmployeeManagement.Domain.Common
{
    public abstract class BaseEntity
    {
        // Primary Key
        public int Id { get; set; }

        // Date when record was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Date when record was last updated
        public DateTime? UpdatedAt { get; set; }
    }
}