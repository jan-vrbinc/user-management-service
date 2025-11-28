using FluentAssertions;
using UserManagementService.Services;

namespace UserManagementService.Tests.Services
{
    public class PasswordServiceTests
    {
        private readonly PasswordService _sut;

        public PasswordServiceTests()
        {
            _sut = new PasswordService();
        }

        [Fact]
        public void HashPassword_ShouldReturnHashString_WhenPasswordIsValid()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash = _sut.HashPassword(password);

            // Assert
            hash.Should().NotBeNullOrEmpty();
            hash.Should().NotBe(password);
        }

        [Fact]
        public void HashPassword_ShouldReturnDifferentHashes_ForSamePassword()
        {
            // Arrange
            var password = "TestPassword123";

            // Act
            var hash1 = _sut.HashPassword(password);
            var hash2 = _sut.HashPassword(password);

            // Assert
            hash1.Should().NotBe(hash2); // BCrypt generates a new salt each time
        }

        [Fact]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
        {
            // Arrange
            var password = "TestPassword123";
            var hash = _sut.HashPassword(password);

            // Act
            var result = _sut.VerifyPassword(password, hash);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
        {
            // Arrange
            var password = "TestPassword123";
            var hash = _sut.HashPassword(password);
            var wrongPassword = "WrongPassword";

            // Act
            var result = _sut.VerifyPassword(wrongPassword, hash);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordHasDifferentCase()
        {
            // Arrange
            var password = "TestPassword123";
            var hash = _sut.HashPassword(password);
            var wrongCasePassword = "testpassword123";

            // Act
            var result = _sut.VerifyPassword(wrongCasePassword, hash);

            // Assert
            result.Should().BeFalse();
        }
    }
}
