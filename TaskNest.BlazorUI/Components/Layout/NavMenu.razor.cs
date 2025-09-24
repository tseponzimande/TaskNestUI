namespace TaskNest.BlazorUI.Components.Layout
{
    public partial class NavMenu
    {
        #region Dependencies

        [Inject]
        private AuthService AuthService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private NotificationService NotificationService { get; set; }

        #endregion

        #region methods

        private async Task Logout()
        {
            await AuthService.LogoutAsync();
            NavigationManager.NavigateTo("/");
            NotificationService.Notify(NotificationSeverity.Info, "Logged Out", "You have been logged out successfully");
        }

        #endregion
    }
}
