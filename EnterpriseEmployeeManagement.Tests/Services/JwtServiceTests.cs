using JwtSvcImpl = EnterpriseEmployeeManagement.Infrastructure.Services.JwtService;
using JwtSettingsModel = EnterpriseEmployeeManagement.Infrastructure.Configuration.JwtSettings;
using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using EnterpriseEmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EnterpriseEmployeeManagement.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtSvcImpl _jwtService;

        public JwtServiceTests()
        {
            var settings = new JwtSettingsModel
            {
                Key = "ThisIsAVeryStrongSecretKeyForTesting123456789",
                Issuer = "EnterpriseEmployeeManagement",
                Audience = "EnterpriseEmployeeManagementUsers",
                ExpireMinutes = 60
            };

            _jwtService = new JwtSvcImpl(
                Options.Create(settings));
        }

        [Fact]
        public void GenerateToken_ShouldReturnAccessToken()
        {
            // Arrange
            var employee = new Employee
            {
                Id = 1,
                FirstName = "Ahmed",
                LastName = "Khan",
                Email = "ahmed@test.com",
                Role = new Role
                {
                    Id = 1,
                    Name = "Admin"
                }
            };

            // Act
            var response = _jwtService.GenerateToken(employee);

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response.AccessToken);
            Assert.Equal(employee.Id, response.EmployeeId);
            Assert.Equal(employee.Email, response.Email);
            Assert.Equal("Admin", response.Role);
        }

        [Fact]
        public void GenerateToken_ShouldContainEmployeeIdClaim()
        {
            // Arrange
            var employee = new Employee
            {
                Id = 10,
                FirstName = "Ahmed",
                LastName = "Khan",
                Email = "ahmed@test.com",
                Role = new Role
                {
                    Id = 1,
                    Name = "Admin"
                }
            };

            // Act
            var response = _jwtService.GenerateToken(employee);

            // Assert
            var handler = new JwtSecurityTokenHandler();

            var token = handler.ReadJwtToken(
                response.AccessToken);

            var claim = token.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.NameIdentifier ||
                     c.Type == "nameid");

            Assert.NotNull(claim);
            Assert.Equal(
                employee.Id.ToString(),
                claim.Value);
        }

        [Fact]
        public void GenerateToken_ShouldContainEmailClaim()
        {
            // Arrange
            var employee = new Employee
            {
                Id = 10,
                FirstName = "Ahmed",
                LastName = "Khan",
                Email = "ahmed@test.com",
                Role = new Role
                {
                    Id = 1,
                    Name = "Admin"
                }
            };

            // Act
            var response = _jwtService.GenerateToken(employee);

            // Assert
            var handler = new JwtSecurityTokenHandler();

            var token = handler.ReadJwtToken(
                response.AccessToken);

            var claim = token.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.Email ||
                     c.Type == "email");

            Assert.NotNull(claim);
            Assert.Equal(
                employee.Email,
                claim.Value);
        }

        [Fact]
        public void GenerateToken_ShouldContainRoleClaim()
        {
            // Arrange
            var employee = new Employee
            {
                Id = 10,
                FirstName = "Ahmed",
                LastName = "Khan",
                Email = "ahmed@test.com",
                Role = new Role
                {
                    Id = 1,
                    Name = "Admin"
                }
            };

            // Act
            var response = _jwtService.GenerateToken(employee);

            // Assert
            var handler = new JwtSecurityTokenHandler();

            var token = handler.ReadJwtToken(
                response.AccessToken);

            var claim = token.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.Role ||
                     c.Type == "role");

            Assert.NotNull(claim);
            Assert.Equal(
                "Admin",
                claim.Value);
        }

        [Fact]
        public void GenerateToken_ShouldSetExpirationTime()
        {
            // Arrange
            var employee = new Employee
            {
                Id = 10,
                FirstName = "Ahmed",
                LastName = "Khan",
                Email = "ahmed@test.com",
                Role = new Role
                {
                    Id = 1,
                    Name = "User"
                }
            };

            var before = DateTime.UtcNow;

            // Act
            var response = _jwtService.GenerateToken(employee);

            var after = DateTime.UtcNow.AddMinutes(61);

            // Assert
            Assert.True(response.ExpiresAt > before);
            Assert.True(response.ExpiresAt < after);
        }
    }
}