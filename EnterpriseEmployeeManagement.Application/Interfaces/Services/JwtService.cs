using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                _jwtSettings.ExpireMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler()
                .WriteToken(token);

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