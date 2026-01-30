using System.ComponentModel.DataAnnotations;

namespace JiraAiTool.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Email { get; set; } 
        [Required]
        public string ApiToken { get; set; } 
        public string? JiraAccountId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
