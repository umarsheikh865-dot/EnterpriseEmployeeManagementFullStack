using JwtSettingsOptions = EnterpriseEmployeeManagement.Infrastructure.Options.JwtSettings;
using AuthResponse = EnterpriseEmployeeManagement.Application.DTOs.Authentication.AuthenticationResponse;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EnterpriseEmployeeManagement.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettingsOptions _jwtSettings;

        public JwtService(IOptions<JwtSettingsOptions> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public AuthResponse GenerateToken(Employee employee)
        {
            var roleName = employee.Role?.Name;

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new InvalidOperationException(
                    "Employee role could not be loaded.");
            }

            // ============================================
            // CLAIMS
            // ============================================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    employee.Id.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    employee.Email),

                new Claim(
                    ClaimTypes.Role,
                    roleName)
            };

            // ============================================
            // CREATE SIGNING KEY
            // ============================================

            var keyBytes = Encoding.UTF8.GetBytes(
                _jwtSettings.Key ?? string.Empty);

            if (keyBytes.Length < 32)
            {
                keyBytes = SHA256.HashData(keyBytes);
            }

            var securityKey =
                new SymmetricSecurityKey(keyBytes);

            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256);

            // ============================================
            // TOKEN EXPIRATION
            // ============================================

            var expires =
                DateTime.UtcNow.AddMinutes(
                    _jwtSettings.ExpireMinutes);

            // ============================================
            // CREATE JWT
            // ============================================

            var token =
                new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: credentials);

            var accessToken =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);

            // ============================================
            // RETURN RESPONSE
            // ============================================

            return new AuthResponse
            {
                AccessToken = accessToken,
                ExpiresAt = expires,
                EmployeeId = employee.Id,
                Email = employee.Email,
                Role = roleName
            };
        }

        public DateTime GetExpirationTime()
        {
            return DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpireMinutes);
        }
    }
}