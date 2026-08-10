namespace EnterpriseEmployeeManagement.Application.DTOs.Authentication
{
    public class AuthenticationResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public int EmployeeId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}