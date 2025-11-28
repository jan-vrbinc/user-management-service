using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Common.DTOs;
using UserManagementService.Data;
using UserManagementService.Models;
using UserManagementService.Services;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;

        public UsersController(AppDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.UserName == createUserDto.UserName))
            {
                return Conflict("Username already exists.");
            }

            var user = new User
            {
                UserName = createUserDto.UserName,
                FullName = createUserDto.FullName,
                Email = createUserDto.Email,
                Mobile = createUserDto.Mobile,
                Language = createUserDto.Language,
                Culture = createUserDto.Culture,
                Password = _passwordService.HashPassword(createUserDto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, MapToDto(user));
        }

        [HttpPatch]
        public async Task<ActionResult<UserDto>> UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            // Using Email to identify user as requested
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == updateUserDto.Email);
            
            if (user == null)
            {
                return NotFound("User with the specified email not found.");
            }

            // Update fields if they are present in the DTO
            if (!string.IsNullOrEmpty(updateUserDto.UserName))
            {
                // Check uniqueness if UserName is changing
                if (user.UserName != updateUserDto.UserName && 
                    await _context.Users.AnyAsync(u => u.UserName == updateUserDto.UserName))
                {
                    return Conflict("Username already exists.");
                }
                user.UserName = updateUserDto.UserName;
            }

            if (!string.IsNullOrEmpty(updateUserDto.FullName))
            {
                user.FullName = updateUserDto.FullName;
            }

            if (updateUserDto.Mobile != null)
            {
                user.Mobile = updateUserDto.Mobile;
            }

            if (updateUserDto.Language != null)
            {
                user.Language = updateUserDto.Language;
            }

            if (updateUserDto.Culture != null)
            {
                user.Culture = updateUserDto.Culture;
            }

            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                user.Password = _passwordService.HashPassword(updateUserDto.Password);
            }

            await _context.SaveChangesAsync();

            return Ok(MapToDto(user));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(user));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users.Select(MapToDto));
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidatePassword(ValidatePasswordDto validatePasswordDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == validatePasswordDto.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            bool isValid = _passwordService.VerifyPassword(validatePasswordDto.Password, user.Password);
            
            if (isValid)
            {
                return Ok("Password is valid.");
            }
            else
            {
                return Unauthorized("Invalid password.");
            }
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Mobile = user.Mobile,
                Language = user.Language,
                Culture = user.Culture
            };
        }
    }
}

