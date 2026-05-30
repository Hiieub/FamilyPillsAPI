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

        [Column("name")]
        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("barcode")]
        [StringLength(100)]
        public string Barcode { get; set; } = string.Empty;

        [Column("total_quantity")]
        [Required]
        public int TotalQuantity { get; set; }

        [Column("unit")]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        [Column("expiry_date")]
        [StringLength(50)]
        public string ExpiryDate { get; set; } = string.Empty;

        [Column("image_path")]
        [StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;

        [Column("quantity")]
        [StringLength(100)]
        public string Quantity { get; set; } = string.Empty;

        [Column("last_updated")]
        [StringLength(50)]
        public string LastUpdated { get; set; } = string.Empty;

        [Column("is_running_low")]
        public bool IsRunningLow { get; set; }

        [Column("is_expired")]
        public bool IsExpired { get; set; }
    }
}
