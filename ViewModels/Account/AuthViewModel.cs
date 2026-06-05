namespace LastBiteNew.ViewModels.Account
{
    // Wraps both auth forms so Login + Register can live in one tabbed view
    // while keeping each strongly-typed for server-side validation.
    public class AuthViewModel
    {
        public LoginViewModel Login { get; set; } = new();
        public RegisterCustomerViewModel Register { get; set; } = new();

        // "login" or "register" — controls which tab is active on load.
        public string ActiveTab { get; set; } = "login";
    }
}
