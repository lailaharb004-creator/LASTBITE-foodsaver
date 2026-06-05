namespace LastBiteNew.ViewModels.Admin
{
    public class ReportsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public int MealsRescued { get; set; }
        public int TotalReservations { get; set; }

        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int NoShowCount { get; set; }

        public int ApprovedRestaurants { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPackages { get; set; }

        public List<TopRestaurantRow> TopRestaurants { get; set; } = new();
    }

    public class TopRestaurantRow
    {
        public string Name { get; set; } = string.Empty;
        public int CompletedPickups { get; set; }
        public decimal Revenue { get; set; }
        public decimal Rating { get; set; }
    }
}
