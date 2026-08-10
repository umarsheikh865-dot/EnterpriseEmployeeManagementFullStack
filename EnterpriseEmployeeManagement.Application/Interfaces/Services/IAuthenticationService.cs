using EnterpriseEmployeeManagement.Application.DTOs.Authentication;

namespace EnterpriseEmployeeManagement.Application.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResponse> RegisterAsync(
            RegisterRequest request);

        Task<AuthenticationResponse> LoginAsync(
            LoginRequest request);

        Task<AuthenticationResponse> RefreshTokenAsync(
            RefreshTokenRequest request);

        Task LogoutAsync(int employeeId);
    }
}