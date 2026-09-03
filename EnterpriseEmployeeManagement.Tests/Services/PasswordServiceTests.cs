using EnterpriseEmployeeManagement.Infrastructure.Services;

namespace EnterpriseEmployeeManagement.Tests.Services
{
    public class PasswordServiceTests
    {
        [Fact]
        public void HashPassword_ShouldReturnHash()
        {
            // Arrange
            var passwordService = new PasswordService();
            var password = "TestPassword123!";

            // Act
            var hash = passwordService.HashPassword(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.NotEqual(password, hash);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
        {
            // Arrange
            var passwordService = new PasswordService();
            var password = "TestPassword123!";
            var hash = passwordService.HashPassword(password);

            // Act
            var result = passwordService.VerifyPassword(
                password,
                hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
        {
            // Arrange
            var passwordService = new PasswordService();
            var password = "TestPassword123!";
            var hash = passwordService.HashPassword(password);

            // Act
            var result = passwordService.VerifyPassword(
                "WrongPassword123!",
                hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HashPassword_ShouldGenerateDifferentHashes_ForSamePassword()
        {
            // Arrange
            var passwordService = new PasswordService();
            var password = "TestPassword123!";

            // Act
            var hash1 = passwordService.HashPassword(password);
            var hash2 = passwordService.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }
    }
}