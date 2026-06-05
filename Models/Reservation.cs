using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LastBiteNew.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }
        public int PackageId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Range(1, 20)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(8,2)")]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal CommissionAmount { get; set; }

        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        [Required, StringLength(20)]
        public string ReservationCode { get; set; } = string.Empty;

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        [StringLength(300)]
        public string? CancellationReason { get; set; }

        public FoodPackage FoodPackage { get; set; } = null!;

        [ForeignKey("CustomerId")]
        public ApplicationUser Customer { get; set; } = null!;

        public Review? Review { get; set; }
    }
}
