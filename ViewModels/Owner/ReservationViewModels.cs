using LastBiteNew.Models;

namespace LastBiteNew.ViewModels.Owner
{
    public class ReservationRowViewModel
    {
        public int ReservationId { get; set; }
        public string ReservationCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime PickupStartTime { get; set; }
        public DateTime PickupEndTime { get; set; }

        public bool IsPickupActive =>
            DateTime.UtcNow >= PickupStartTime && DateTime.UtcNow <= PickupEndTime;

        public bool IsPickupPassed => DateTime.UtcNow > PickupEndTime;
    }
}
