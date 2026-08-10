using System.Security.Cryptography;
using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using EnterpriseEmployeeManagement.Application.Interfaces.Repositories;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Domain.Entities;

namespace EnterpriseEmployeeManagement.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;

        public AuthenticationService(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        // ============================================
        // GENERATE REFRESH TOKEN
        // ============================================

        private static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var randomNumberGenerator =
                RandomNumberGenerator.Create();

            randomNumberGenerator.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes);
        }

        // ============================================
        // REGISTER
        // ============================================

        public async Task<AuthenticationResponse> RegisterAsync(
            RegisterRequest request)
        {
            var employeeRepository =
                _unitOfWork.Repository<Employee>();

            var employees =
                await employeeRepository.GetAllAsync();

            var existingEmployee = employees
                .FirstOrDefault(e =>
                    e.Email.ToLower() ==
                    request.Email.ToLower());

            if (existingEmployee is not null)
            {
                throw new InvalidOperationException(
                    "An employee with this email already exists.");
            }

            var employee = new Employee
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,

                PasswordHash =
                    _passwordService.HashPassword(
                        request.Password),

                DepartmentId = request.DepartmentId,
                RoleId = request.RoleId
            };

            await employeeRepository.AddAsync(employee);

            await _unitOfWork.SaveChangesAsync();

            return new AuthenticationResponse
            {
                EmployeeId = employee.Id,
                Email = employee.Email,
                ExpiresAt = DateTime.UtcNow
            };
        }

        // ============================================
        // LOGIN
        // ============================================

        public async Task<AuthenticationResponse> LoginAsync(
            LoginRequest request)
        {
            var employee =
                await _unitOfWork.Employees
                    .GetByEmailAsync(request.Email);

            if (employee is null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            var passwordValid =
                _passwordService.VerifyPassword(
                    request.Password,
                    employee.PasswordHash);

            if (!passwordValid)
            {
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            // Generate access token
            var authenticationResponse =
                _jwtService.GenerateToken(employee);

            // Generate refresh token
            var refreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                EmployeeId = employee.Id
            };

            // Save refresh token
            var refreshTokenRepository =
                _unitOfWork.Repository<RefreshToken>();

            await refreshTokenRepository.AddAsync(
                refreshToken);

            await _unitOfWork.SaveChangesAsync();

            authenticationResponse.RefreshToken =
                refreshToken.Token;

            return authenticationResponse;
        }

        // ============================================
        // REFRESH ACCESS TOKEN
        // ============================================

        public async Task<AuthenticationResponse> RefreshTokenAsync(
            RefreshTokenRequest request)
        {
            var refreshTokenRepository =
                _unitOfWork.Repository<RefreshToken>();

            var refreshTokens =
                await refreshTokenRepository.GetAllAsync();

            var refreshToken = refreshTokens
                .FirstOrDefault(r =>
                    r.Token == request.RefreshToken);

            if (refreshToken is null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token.");
            }

            if (refreshToken.IsRevoked)
            {
                throw new UnauthorizedAccessException(
                    "Refresh token has been revoked.");
            }

            if (refreshToken.Expires <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException(
                    "Refresh token has expired.");
            }

            var employee =
                await _unitOfWork.Employees
                    .GetByIdAsync(refreshToken.EmployeeId);

            if (employee is null)
            {
                throw new UnauthorizedAccessException(
                    "Employee not found.");
            }

            // Generate new access token
            var authenticationResponse =
                _jwtService.GenerateToken(employee);

            // Revoke old refresh token
            refreshToken.IsRevoked = true;

            // Generate new refresh token
            var newRefreshToken = new RefreshToken
            {
                Token = GenerateRefreshToken(),
                Expires = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                EmployeeId = employee.Id
            };

            await refreshTokenRepository.AddAsync(
                newRefreshToken);

            await _unitOfWork.SaveChangesAsync();

            authenticationResponse.RefreshToken =
                newRefreshToken.Token;

            return authenticationResponse;
        }

        // ============================================
        // LOGOUT
        // ============================================

        public async Task LogoutAsync(int employeeId)
        {
            var refreshTokenRepository =
                _unitOfWork.Repository<RefreshToken>();

            var refreshTokens =
                await refreshTokenRepository.GetAllAsync();

            var employeeTokens = refreshTokens
                .Where(r =>
                    r.EmployeeId == employeeId &&
                    !r.IsRevoked)
                .ToList();

            foreach (var refreshToken in employeeTokens)
            {
                refreshToken.IsRevoked = true;
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}