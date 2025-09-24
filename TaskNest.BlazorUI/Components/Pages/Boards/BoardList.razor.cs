namespace TaskNest.BlazorUI.Components.Pages.Boards
{
    [Authorize]
    public partial class BoardList
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

        private List<BoardDto> boards;
        private bool loading = true;
        private BoardDto newBoard = new();

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            await LoadBoards();
        }

        private async Task LoadBoards()
        {
            try
            {
                loading = true;
                boards = (await ApiService.GetAsync<List<BoardDto>>("Boards")).ToList();
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to load boards");
                Console.WriteLine($"Error loading boards: {ex.Message}");
            }
            finally
            {
                loading = false;
            }
        }

        private void ViewBoard(Guid id)
        {
            NavigationManager.NavigateTo($"/boards/{id}");
        }

        private async Task ShowCreateBoardDialog()
        {
            var result = await DialogService.OpenAsync<BoardDialog>("Create Board",
                new Dictionary<string, object> { { "Board", new BoardDto() } },
                new DialogOptions { Width = "500px", Height = "auto", CloseDialogOnOverlayClick = true });

            if (result != null)
            {
                await LoadBoards();
                NotificationService.Notify(NotificationSeverity.Success, "Success", "Board created successfully");
            }
        }

        private async Task EditBoard(BoardDto board)
        {
            var result = await DialogService.OpenAsync<BoardDialog>("Edit Board",
                new Dictionary<string, object> { { "Board", board } },
                new DialogOptions { Width = "500px", Height = "auto", CloseDialogOnOverlayClick = true });

            if (result != null)
            {
                await LoadBoards();
                NotificationService.Notify(NotificationSeverity.Success, "Success", "Board updated successfully");
            }
        }

        private async Task ConfirmDeleteBoard(Guid id)
        {
            var result = await DialogService.Confirm("Are you sure you want to delete this board?", "Delete Board",
                new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

            if (result == true)
            {
                try
                {
                    await ApiService.DeleteAsync($"Boards/{id}");
                    await LoadBoards();
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Board deleted successfully");
                }
                catch (Exception ex)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to delete board");
                    Console.WriteLine($"Error deleting board: {ex.Message}");
                }
            }
        }
        #endregion
    }
}
