namespace TaskNest.BlazorUI.Components.Pages.Dialogs
{
    public partial class BoardDialog
    {
        #region Dependencies

        [Inject]
        public ApiService ApiService { get; set; }

        [Inject]
        public DialogService DialogService { get; set; }

        [Inject]
        public NotificationService NotificationService { get; set; }
        #endregion

        #region Parameters

        [Parameter] 
        public BoardDto Board { get; set; } = new();

        #endregion

        #region Methods

        private async Task Submit()
        {
            try
            {
                if (Board.Id == Guid.Empty)
                {
                    // Create new board
                    var createdBoard = await ApiService.PostAsync<BoardDto, BoardDto>("Boards", Board);
                    DialogService.Close(createdBoard);
                }
                else
                {
                    // Update existing board
                    await ApiService.PutAsync($"Boards/{Board.Id}", Board);
                    DialogService.Close(Board);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to save board");
                Console.WriteLine($"Error saving board: {ex.Message}");
            }
        }
        #endregion
    }
}
