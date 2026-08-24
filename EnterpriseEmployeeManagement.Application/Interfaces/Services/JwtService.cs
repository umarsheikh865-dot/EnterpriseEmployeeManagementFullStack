using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
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
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public AuthenticationResponse GenerateToken(Employee employee)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    employee.Id.ToString()),

                new Claim(
                    JwtRegisteredClaimNames.Email,
                    employee.Email),

                new Claim(
                    ClaimTypes.Role,
                    employee.Role?.Name ?? string.Empty)
            };

            // ============================================
            // JWT KEY
            // ============================================

            var keyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            if (keyBytes.Length < 32)
            {
                keyBytes = SHA256.HashData(keyBytes);
            }

            var key = new SymmetricSecurityKey(keyBytes);

            // ============================================
            // SIGNING CREDENTIALS
            // ============================================

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // ============================================
            // EXPIRATION
            // ============================================

            var expires = DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpireMinutes);

            // ============================================
            // CREATE TOKEN
            // ============================================

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            // ============================================
            // CONVERT TOKEN TO STRING
            // ============================================

            var accessToken = new JwtSecurityTokenHandler()
                .WriteToken(token);

            // ============================================
            // RESPONSE
            // ============================================

            return new AuthenticationResponse
            {
                AccessToken = accessToken,
                ExpiresAt = expires,
                EmployeeId = employee.Id,
                Email = employee.Email,
                Role = employee.Role?.Name ?? string.Empty
            };
        }

        public DateTime GetExpirationTime()
        {
            return DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpireMinutes);
        }
    }
}