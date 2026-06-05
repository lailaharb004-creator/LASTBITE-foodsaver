namespace LastBiteNew.ViewModels.Admin
{
    public class UserRowViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public System.DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public int Reservations { get; set; }
        public int Reviews { get; set; }
    }
}
