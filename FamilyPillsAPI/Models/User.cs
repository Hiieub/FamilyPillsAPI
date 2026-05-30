using System.ComponentModel.DataAnnotations;

namespace FamilyPillsAPI.Models
{
    /// <summary>
    /// User model for authentication and profile
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; } // This will store bcrypt hash

        [StringLength(255)]
        public string FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
