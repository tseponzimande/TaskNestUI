namespace TaskNest.BlazorUI.Components.Pages.Boards
{
    [Authorize]
    public partial class BoardUserManagement
    {
        #region Dependencies

        [Inject]
        public ApiService ApiService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public DialogService DialogService { get; set; }

        [Inject]
        public NotificationService NotificationService { get; set; }

        #endregion

        #region Parameters

        [Parameter]
        public Guid BoardId { get; set; }

        #endregion

        #region Properties

        private List<BoardUserDto> boardUsers;

        private AddBoardUserDto newUser = new();

        private bool loading = true;

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            await LoadBoardUsers();
        }

        private async Task LoadBoardUsers()
        {
            try
            {
                loading = true;
                boardUsers = await ApiService.GetAsync<List<BoardUserDto>>($"boards/{BoardId}/users");
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to load board users");
                Console.WriteLine($"Error loading board users: {ex.Message}");
            }
            finally
            {
                loading = false;
            }
        }

        private async Task AddUser()
        {
            try
            {
                await ApiService.PostAsync<AddBoardUserDto, BoardUserDto>($"boards/{BoardId}/users", newUser);
                newUser = new AddBoardUserDto();
                await LoadBoardUsers();
                NotificationService.Notify(NotificationSeverity.Success, "Success", "User added successfully");
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", ex.Message);
                Console.WriteLine($"Error adding user: {ex.Message}");
            }
        }

        private async Task UpdateUserRole(string userId, BoardUserRole role)
        {
            try
            {
                await ApiService.PutAsync($"boards/{BoardId}/users/{userId}", role);
                NotificationService.Notify(NotificationSeverity.Success, "Success", "User role updated");
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to update user role");
                Console.WriteLine($"Error updating user role: {ex.Message}");
                await LoadBoardUsers(); // Reload to restore correct state
            }
        }

        private async Task ConfirmRemoveUser(string userId)
        {
            var result = await DialogService.Confirm("Are you sure you want to remove this user from the board?",
                "Remove User", new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

            if (result == true)
            {
                try
                {
                    await ApiService.DeleteAsync($"boards/{BoardId}/users/{userId}");
                    await LoadBoardUsers();
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "User removed successfully");
                }
                catch (Exception ex)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to remove user");
                    Console.WriteLine($"Error removing user: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
