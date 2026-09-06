using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Domain.Entities;
using EnterpriseEmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace EnterpriseEmployeeManagement.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            ApplicationDbContext context,
            IPasswordService passwordService,
            IJwtService jwtService,
            ILogger<AuthenticationService> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _logger = logger;
        }

        // =========================================================
        // REGISTER
        // =========================================================

        public async Task<AuthenticationResponse> RegisterAsync(
            RegisterRequest request)
        {
            // Check if email already exists
            var existingEmployee =
                await _context.Employees
                    .FirstOrDefaultAsync(
                        e => e.Email == request.Email);

            if (existingEmployee != null)
            {
                throw new InvalidOperationException(
                    "An employee with this email already exists.");
            }

            // Check Department
            var department =
                await _context.Departments
                    .FirstOrDefaultAsync(
                        d => d.Id == request.DepartmentId);

            if (department == null)
            {
                throw new InvalidOperationException(
                    "The selected department does not exist.");
            }

            // Check Role
            var role =
                await _context.Roles
                    .FirstOrDefaultAsync(
                        r => r.Id == request.RoleId);

            if (role == null)
            {
                throw new InvalidOperationException(
                    "The selected role does not exist.");
            }

            // Create employee
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

            await _context.Employees.AddAsync(employee);

            await _context.SaveChangesAsync();

            // Load Role for JWT claim
            await _context.Entry(employee)
                .Reference(e => e.Role)
                .LoadAsync();

            // Generate access token
            var response =
                _jwtService.GenerateToken(employee);

            // Generate refresh token
            var refreshToken =
                CreateRefreshToken(employee.Id);

            await _context.RefreshTokens.AddAsync(
                refreshToken);

            await _context.SaveChangesAsync();

            response.RefreshToken =
                refreshToken.Token;

            return response;
        }

        // =========================================================
        // LOGIN
        // =========================================================

        public async Task<AuthenticationResponse> LoginAsync(
            LoginRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with empty email or password.");
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            var employee =
                await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == request.Email);

            if (employee != null)
            {
                // Load related navigation properties for JWT claims
                await _context.Entry(employee)
                    .Reference(e => e.Role)
                    .LoadAsync();

                await _context.Entry(employee)
                    .Reference(e => e.Department)
                    .LoadAsync();
            }

            if (employee == null)
            {
                _logger.LogWarning($"Login attempt failed: No employee found with email: {request.Email}");
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            // Verify password hash exists
            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                _logger.LogError($"Critical: Employee {employee.Id} ({employee.Email}) has no password hash stored.");
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            // Verify password
            var passwordValid =
                _passwordService.VerifyPassword(
                    request.Password,
                    employee.PasswordHash);

            if (!passwordValid)
            {
                _logger.LogWarning($"Login attempt failed: Invalid password for email: {request.Email}");
                throw new UnauthorizedAccessException(
                    "Invalid email or password.");
            }

            // Password validated successfully
            _logger.LogInformation($"Successful login for employee: {employee.Email} (ID: {employee.Id})");

            // Generate access token
            var response =
                _jwtService.GenerateToken(employee);

            // Generate refresh token
            var refreshToken =
                CreateRefreshToken(employee.Id);

            await _context.RefreshTokens.AddAsync(
                refreshToken);

            await _context.SaveChangesAsync();

            response.RefreshToken =
                refreshToken.Token;

            return response;
        }

        // =====================================  ====================
        // REFRESH TOKEN
        // =========================================================

        public async Task<AuthenticationResponse> RefreshTokenAsync(
            RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                request.RefreshToken))
            {
                throw new UnauthorizedAccessException(
                    "Refresh token is required.");
            }

            // Find refresh token and employee
            var storedToken =
                await _context.RefreshTokens
                    .Include(r => r.Employee)
                    .ThenInclude(e => e.Role)
                    .FirstOrDefaultAsync(
                        r => r.Token == request.RefreshToken);

            if (storedToken == null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token.");
            }

            // Check token status
            if (!storedToken.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Refresh token is expired or revoked.");
            }

            // =====================================================
            // REVOKE OLD REFRESH TOKEN
            // =====================================================

            storedToken.RevokedAt =
                DateTime.UtcNow;

            // =====================================================
            // CREATE NEW ACCESS TOKEN
            // =====================================================

            var response =
                _jwtService.GenerateToken(
                    storedToken.Employee);

            // =====================================================
            // CREATE NEW REFRESH TOKEN
            // =====================================================

            var newRefreshToken =
                CreateRefreshToken(
                    storedToken.EmployeeId);

            await _context.RefreshTokens.AddAsync(
                newRefreshToken);

            await _context.SaveChangesAsync();

            response.RefreshToken =
                newRefreshToken.Token;

            return response;
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        public async Task LogoutAsync(int employeeId)
        {
            var refreshTokens =
                await _context.RefreshTokens
                    .Where(r =>
                        r.EmployeeId == employeeId &&
                        r.RevokedAt == null)
                    .ToListAsync();

            foreach (var refreshToken in refreshTokens)
            {
                refreshToken.RevokedAt =
                    DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // =========================================================
        // CREATE REFRESH TOKEN
        // =========================================================

        private RefreshToken CreateRefreshToken(
            int employeeId)
        {
            return new RefreshToken
            {
                Token = GenerateRefreshToken(),

                CreatedAt =
                    DateTime.UtcNow,

                ExpiresAt =
                    DateTime.UtcNow.AddDays(7),

                EmployeeId = employeeId
            };
        }

        // =========================================================
        // GENERATE RANDOM REFRESH TOKEN
        // =========================================================

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));
        }
    }
}