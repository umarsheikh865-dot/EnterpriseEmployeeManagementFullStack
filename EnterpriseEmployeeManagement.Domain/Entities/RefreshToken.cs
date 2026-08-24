namespace EnterpriseEmployeeManagement.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public int EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        public bool IsRevoked =>
            RevokedAt.HasValue;

        public bool IsExpired =>
            DateTime.UtcNow >= ExpiresAt;

        public bool IsActive =>
            !IsRevoked && !IsExpired;
    }
}