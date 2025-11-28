using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Models
{
    public class ApiClient
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string ApiKey { get; set; } = string.Empty;
    }
}

