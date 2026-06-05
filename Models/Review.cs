namespace LastBiteNew.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        public int ReservationId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        public int RestaurantId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        public bool IsVisible { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Reservation Reservation { get; set; } = null!;

        [ForeignKey("CustomerId")]
        public ApplicationUser Customer { get; set; } = null!;

        public Restaurant Restaurant { get; set; } = null!;
    }

}
