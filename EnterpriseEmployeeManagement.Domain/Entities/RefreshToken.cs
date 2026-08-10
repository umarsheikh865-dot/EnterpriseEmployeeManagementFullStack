using EnterpriseEmployeeManagement.Domain.Common;

namespace EnterpriseEmployeeManagement.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        // Refresh Token
        public string Token { get; set; } = string.Empty;

        // Expiration Date
        public DateTime Expires { get; set; }

        // Is token revoked?
        public bool IsRevoked { get; set; }

        // Foreign Key
        public int EmployeeId { get; set; }

        // Navigation Property
        public Employee Employee { get; set; } = null!;
    }
}