using EnterpriseEmployeeManagement.Application.DTOs.Authentication;
using EnterpriseEmployeeManagement.Application.Interfaces.Services;
using EnterpriseEmployeeManagement.Domain.Entities;
using EnterpriseEmployeeManagement.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using AuthServiceImplementation = EnterpriseEmployeeManagement.Infrastructure.Services.AuthenticationService;

namespace EnterpriseEmployeeManagement.Tests.Services
{
    public class AuthenticationServiceTests
    {
        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            using var context =
                TestDbContextFactory.Create();

            var passwordService =
                new Mock<IPasswordService>();

            var jwtService =
                new Mock<IJwtService>();

            var logger =
                new Mock<ILogger<AuthServiceImplementation>>();

            var passwordHash = "hashed-password";

            var employee = new Employee
            {
                Id = 1,
                FirstName = "Ahmed",
                LastName = "Khan",
                Email = "ahmed@test.com",
                PasswordHash = passwordHash,
                DepartmentId = 1,
                RoleId = 1,
                Role = new Role
                {
                    Id = 1,
                    Name = "Admin"
                }
            };

            context.Employees.Add(employee);

            await context.SaveChangesAsync();

            passwordService
                .Setup(x => x.VerifyPassword(
                    "Password123!",
                    passwordHash))
                .Returns(true);

            jwtService
                .Setup(x => x.GenerateToken(
                    It.IsAny<Employee>()))
                .Returns(new AuthenticationResponse
                {
                    AccessToken = "test-access-token",
                    EmployeeId = employee.Id,
                    Email = employee.Email,
                    Role = "Admin",
                    ExpiresAt =
                        DateTime.UtcNow.AddMinutes(60)
                });

            var service =
                new AuthServiceImplementation(
                    context,
                    passwordService.Object,
                    jwtService.Object,
                    logger.Object);

            var request = new LoginRequest
            {
                Email = "ahmed@test.com",
                Password = "Password123!"
            };

            // Act
            var result =
                await service.LoginAsync(request);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(
                "test-access-token",
                result.AccessToken);

            Assert.Equal(
                employee.Email,
                result.Email);

            Assert.Equal(
                "Admin",
                result.Role);

            passwordService.Verify(
                x => x.VerifyPassword(
                    "Password123!",
                    passwordHash),
                Times.Once);

            jwtService.Verify(
                x => x.GenerateToken(
                    It.IsAny<Employee>()),
                Times.Once);
        }


        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorized_WhenEmployeeDoesNotExist()
        {
            // Arrange
            using var context =
                TestDbContextFactory.Create();

            var passwordService =
                new Mock<IPasswordService>();

            var jwtService =
                new Mock<IJwtService>();

            var logger =
                new Mock<ILogger<AuthServiceImplementation>>();

            var service =
                new AuthServiceImplementation(
                    context,
                    passwordService.Object,
                    jwtService.Object,
                    logger.Object);

            var request = new LoginRequest
            {
                Email = "notfound@test.com",
                Password = "Password123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.LoginAsync(request));
        }


        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorized_WhenPasswordIsWrong()
        {
            // Arrange
            using var context =
                TestDbContextFactory.Create();

            var passwordService =
                new Mock<IPasswordService>();

            var jwtService =
                new Mock<IJwtService>();

            var logger =
                new Mock<ILogger<AuthServiceImplementation>>();

            var employee = new Employee
            {
                Id = 1,
                FirstName = "Ahmed",
                LastName = "Khan",
                Email = "ahmed@test.com",
                PasswordHash = "hashed-password",
                DepartmentId = 1,
                RoleId = 1,
                Role = new Role
                {
                    Id = 1,
                    Name = "Admin"
                }
            };

            context.Employees.Add(employee);

            await context.SaveChangesAsync();

            passwordService
                .Setup(x => x.VerifyPassword(
                    "WrongPassword!",
                    "hashed-password"))
                .Returns(false);

            var service =
                new AuthServiceImplementation(
                    context,
                    passwordService.Object,
                    jwtService.Object,
                    logger.Object);

            var request = new LoginRequest
            {
                Email = "ahmed@test.com",
                Password = "WrongPassword!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.LoginAsync(request));

            jwtService.Verify(
                x => x.GenerateToken(
                    It.IsAny<Employee>()),
                Times.Never);
        }


        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorized_WhenEmailIsEmpty()
        {
            // Arrange
            using var context =
                TestDbContextFactory.Create();

            var passwordService =
                new Mock<IPasswordService>();

            var jwtService =
                new Mock<IJwtService>();

            var logger =
                new Mock<ILogger<AuthServiceImplementation>>();

            var service =
                new AuthServiceImplementation(
                    context,
                    passwordService.Object,
                    jwtService.Object,
                    logger.Object);

            var request = new LoginRequest
            {
                Email = "",
                Password = "Password123!"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.LoginAsync(request));
        }


        [Fact]
        public async Task RefreshTokenAsync_ShouldReturnNewAccessToken()
        {
            // Arrange
            using var context =
                TestDbContextFactory.Create();

            var passwordService =
                new Mock<IPasswordService>();

            var jwtService =
                new Mock<IJwtService>();

            var logger =
                new Mock<ILogger<AuthServiceImplementation>>();

            var employee = new Employee
            {
                Id = 1,
                FirstName = "Ahmed",
                LastName = "Khan",
                Email = "ahmed@test.com",
                PasswordHash = "hash",
                DepartmentId = 1,
                RoleId = 1,
                Role = new Role
                {
                    Id = 1,
                    Name = "Admin"
                }
            };

            context.Employees.Add(employee);

            var refreshToken = new RefreshToken
            {
                Token = "old-refresh-token",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                EmployeeId = employee.Id,
                Employee = employee
            };

            context.RefreshTokens.Add(refreshToken);

            await context.SaveChangesAsync();

            jwtService
                .Setup(x => x.GenerateToken(
                    It.IsAny<Employee>()))
                .Returns(new AuthenticationResponse
                {
                    AccessToken = "new-access-token",
                    EmployeeId = employee.Id,
                    Email = employee.Email,
                    Role = "Admin",
                    ExpiresAt =
                        DateTime.UtcNow.AddMinutes(60)
                });

            var service =
                new AuthServiceImplementation(
                    context,
                    passwordService.Object,
                    jwtService.Object,
                    logger.Object);

            var request = new RefreshTokenRequest
            {
                RefreshToken = "old-refresh-token"
            };

            // Act
            var result =
                await service.RefreshTokenAsync(request);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(
                "new-access-token",
                result.AccessToken);

            Assert.NotEqual(
                "old-refresh-token",
                result.RefreshToken);

            Assert.NotNull(
                refreshToken.RevokedAt);
        }


        [Fact]
        public async Task LogoutAsync_ShouldRevokeActiveRefreshTokens()
        {
            // Arrange
            using var context =
                TestDbContextFactory.Create();

            var passwordService =
                new Mock<IPasswordService>();

            var jwtService =
                new Mock<IJwtService>();

            var logger =
                new Mock<ILogger<AuthServiceImplementation>>();

            var employee = new Employee
            {
                Id = 1,
                FirstName = "Ahmed",
                LastName = "Khan",
                Email = "ahmed@test.com",
                PasswordHash = "hash",
                DepartmentId = 1,
                RoleId = 1
            };

            context.Employees.Add(employee);

            var refreshToken = new RefreshToken
            {
                Token = "active-token",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                EmployeeId = employee.Id
            };

            context.RefreshTokens.Add(refreshToken);

            await context.SaveChangesAsync();

            var service =
                new AuthServiceImplementation(
                    context,
                    passwordService.Object,
                    jwtService.Object,
                    logger.Object);

            // Act
            await service.LogoutAsync(employee.Id);

            // Assert
            Assert.NotNull(
                refreshToken.RevokedAt);
        }
    }
}