using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FamilyPillsAPI.Models
{
    [Table("medicines")]
    public class Medicine
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("barcode")]
        [StringLength(100)]
        public string? Barcode { get; set; }

        [Column("total_quantity")]
        [Required]
        public int TotalQuantity { get; set; }

        [Column("unit")]
        [StringLength(50)]
        public string? Unit { get; set; }

        [Column("expiry_date")]
        [StringLength(50)]
        public string? ExpiryDate { get; set; }

        [Column("image_path")]
        [StringLength(500)]
        public string? ImagePath { get; set; }

        [Column("quantity")]
        [StringLength(100)]
        public string? Quantity { get; set; }

        [Column("last_updated")]
        [StringLength(50)]
        public string? LastUpdated { get; set; }

        [Column("is_running_low")]
        public bool IsRunningLow { get; set; }

        [Column("is_expired")]
        public bool IsExpired { get; set; }

        public User? User { get; set; }
    }
}
