using LastBiteNew.Models;

namespace LastBiteNew.ViewModels.Owner
{
    public class OwnerDashboardViewModel
    {
        public Restaurant Restaurant { get; set; } = null!;
        public int ActivePackages { get; set; }
        public int TotalPackages { get; set; }
        public int TotalReservations { get; set; }
        public int CompletedReservations { get; set; }
        public decimal TotalRevenue { get; set; }
        public int MealsSaved { get; set; }
        public List<RecentReservationRow> RecentReservations { get; set; } = new();
    }

    public class RecentReservationRow
    {
        public string ReservationCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime ReservedAt { get; set; }
    }
}
