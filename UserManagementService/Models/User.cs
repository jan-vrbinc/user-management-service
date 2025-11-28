using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Models
{
    public class User
    {
        public int Id { get; set; }
        
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
}

