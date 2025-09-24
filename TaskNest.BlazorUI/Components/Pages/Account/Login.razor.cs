namespace TaskNest.BlazorUI.Components.Pages.Account
{
    public partial class Login
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

        #region Parameters

        private LoginDto loginModel = new();

        #endregion

        #region Methods
        private async Task UserLogin()
        {
            try
            {
                var result = await AuthService.LoginAsync(loginModel);

                if (result)
                {
                    NavigationManager.NavigateTo("/boards");
                }
                else
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Login Failed", "Invalid email or password");
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "An error occurred during login");
                Console.WriteLine($"Login error: {ex.Message}");
            }
        }
        #endregion
    }
}
