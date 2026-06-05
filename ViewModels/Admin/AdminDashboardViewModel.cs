namespace LastBiteNew.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalApprovedRestaurants { get; set; }
        public int PendingApprovals { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalReservations { get; set; }
        public int CompletedReservations { get; set; }
        public decimal TotalCommissionEarned { get; set; }
        public int MealsSaved { get; set; }
        public List<RestaurantRowViewModel> PendingRestaurants { get; set; } = new();
    }
}
