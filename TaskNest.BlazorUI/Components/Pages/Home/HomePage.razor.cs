namespace TaskNest.BlazorUI.Components.Pages.Home
{
    public partial class HomePage
    {
        #region Dependencies

        [Inject]
        private AuthService AuthService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private NotificationService NotificationService { get; set; }

        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        #endregion

        #region Navigation methods
        private void NavigateToRegister()
        {
            NavigationManager.NavigateTo("/register");
        }

        private void NavigateToLogin()
        {
            NavigationManager.NavigateTo("/login");
        }
        #endregion
    }
}
