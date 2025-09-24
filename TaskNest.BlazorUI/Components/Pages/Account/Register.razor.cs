namespace TaskNest.BlazorUI.Components.Pages.Account
{
    public partial class Register
    {
        #region Dependencies

        [Inject]
        public AuthService AuthService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public DialogService DialogService { get; set; }

        [Inject]
        public NotificationService NotificationService { get; set; }

        #endregion

        #region Properties and Fields

        private RegisterDto registerModel = new();
        private string confirmPassword;

        #endregion

        #region Methods
        private async Task UserRegister()
        {
            try
            {
                var result = await AuthService.RegisterAsync(registerModel);

                if (result)
                {
                    NotificationService.Notify(NotificationSeverity.Success, "Registration Successful", "You can now login with your credentials");
                    NavigationManager.NavigateTo("/login");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Registration Failed", "Please try again with different credentials");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "An error occurred during registration");
                Console.WriteLine($"Registration error: {ex.Message}");
            }
        }
        #endregion
    }
}
