using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagementService.Controllers;
using UserManagementService.Data;
using UserManagementService.Common.DTOs;
using UserManagementService.Models;
using UserManagementService.Services;

namespace UserManagementService.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly UsersController _sut;

        public UsersControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;

            _context = new AppDbContext(options);
            _passwordServiceMock = new Mock<IPasswordService>();
            _sut = new UsersController(_context, _passwordServiceMock.Object);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnCreated_WhenUserIsValid()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                UserName = "testuser",
                FullName = "Test User",
                Email = "test@example.com",
                Password = "password123"
            };

            _passwordServiceMock.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hashed_password");

            // Act
            var result = await _sut.CreateUser(dto);

            // Assert
            var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var returnDto = createdAtActionResult.Value.Should().BeOfType<UserDto>().Subject;
            returnDto.UserName.Should().Be(dto.UserName);
            
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
            userInDb.Should().NotBeNull();
            userInDb!.Password.Should().Be("hashed_password");
        }

        [Fact]
        public async Task CreateUser_ShouldReturnConflict_WhenUsernameExists()
        {
            // Arrange
            _context.Users.Add(new User
            {
                UserName = "existinguser",
                FullName = "Existing",
                Email = "existing@example.com",
                Password = "hash"
            });
            await _context.SaveChangesAsync();

            var dto = new CreateUserDto
            {
                UserName = "existinguser", // Duplicate
                FullName = "New User",
                Email = "new@example.com",
                Password = "password123"
            };

            // Act
            var result = await _sut.CreateUser(dto);

            // Assert
            result.Result.Should().BeOfType<ConflictObjectResult>();
        }

        [Fact]
        public async Task GetUser_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                UserName = "testuser",
                FullName = "Test User",
                Email = "test@example.com",
                Password = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.GetUser(user.Id);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnDto = okResult.Value.Should().BeOfType<UserDto>().Subject;
            returnDto.Id.Should().Be(user.Id);
        }

        [Fact]
        public async Task GetUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Act
            var result = await _sut.GetUser(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNoContent_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                UserName = "deleteuser",
                FullName = "Delete User",
                Email = "delete@example.com",
                Password = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _sut.DeleteUser(user.Id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            _context.Users.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Act
            var result = await _sut.DeleteUser(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dto = new UpdateUserDto { Email = "nonexistent@example.com" };

            // Act
            var result = await _sut.UpdateUser(dto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task UpdateUser_ShouldUpdateFields_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                UserName = "olduser",
                FullName = "Old Name",
                Email = "update@example.com",
                Mobile = "123",
                Password = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new UpdateUserDto
            {
                Email = "update@example.com",
                FullName = "New Name",
                Mobile = "456"
            };

            // Act
            var result = await _sut.UpdateUser(dto);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            
            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser!.FullName.Should().Be("New Name");
            updatedUser.Mobile.Should().Be("456");
            updatedUser.UserName.Should().Be("olduser"); // Should not change
        }

        [Fact]
        public async Task ValidatePassword_ShouldReturnOk_WhenPasswordIsValid()
        {
            // Arrange
            var user = new User
            {
                UserName = "user",
                FullName = "User",
                Email = "user@example.com",
                Password = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _passwordServiceMock.Setup(x => x.VerifyPassword("password", "hash")).Returns(true);

            var dto = new ValidatePasswordDto { Email = user.Email, Password = "password" };

            // Act
            var result = await _sut.ValidatePassword(dto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ValidatePassword_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
        {
            // Arrange
            var user = new User
            {
                UserName = "user",
                FullName = "User",
                Email = "user@example.com",
                Password = "hash"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _passwordServiceMock.Setup(x => x.VerifyPassword("wrongpassword", "hash")).Returns(false);

            var dto = new ValidatePasswordDto { Email = user.Email, Password = "wrongpassword" };

            // Act
            var result = await _sut.ValidatePassword(dto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task ValidatePassword_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dto = new ValidatePasswordDto { Email = "nonexistent@example.com", Password = "password" };

            // Act
            var result = await _sut.ValidatePassword(dto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
