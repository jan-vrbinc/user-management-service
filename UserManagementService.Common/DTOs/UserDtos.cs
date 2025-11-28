using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Common.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Culture { get; set; } = string.Empty;
    }

    public class CreateUserDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string Mobile { get; set; } = string.Empty;
        
        public string Language { get; set; } = string.Empty;
        
        public string Culture { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? UserName { get; set; }
        
        public string? FullName { get; set; }
        
        public string? Mobile { get; set; }
        
        public string? Language { get; set; }
        
        public string? Culture { get; set; }
        
        public string? Password { get; set; }
    }

    public class ValidatePasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

