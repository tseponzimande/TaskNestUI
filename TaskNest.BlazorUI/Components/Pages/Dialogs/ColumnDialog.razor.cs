namespace TaskNest.BlazorUI.Components.Pages.Dialogs
{
    public partial class ColumnDialog
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
        public BoardColumnDto Column { get; set; } = new();

        #endregion

        #region Methods

        private async Task Submit()
        {
            try
            {
                if (Column.Id == Guid.Empty)
                {
                    // Create new column
                    var createdColumn = await ApiService.PostAsync<BoardColumnDto, BoardColumnDto>("BoardColumns", Column);
                    DialogService.Close(createdColumn);
                }
                else
                {
                    // Update existing column
                    await ApiService.PutAsync($"BoardColumns/{Column.Id}", Column);
                    DialogService.Close(Column);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to save column");
                Console.WriteLine($"Error saving column: {ex.Message}");
            }
        }

        #endregion

    }
}
