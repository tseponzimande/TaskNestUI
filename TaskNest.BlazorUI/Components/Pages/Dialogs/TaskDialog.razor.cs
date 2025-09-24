namespace TaskNest.BlazorUI.Components.Pages.Dialogs
{
    public partial class TaskDialog
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
        public TaskItemDto Task { get; set; } = new();

        [Parameter]
        public List<BoardColumnDto> Columns { get; set; } = new();

        #endregion

        #region

        private async Task Submit()
        {
            try
            {
                if (Task.Id == Guid.Empty)
                {
                    // Create new task
                    var createdTask = await ApiService.PostAsync<TaskItemDto, TaskItemDto>("TaskItems", Task);
                    DialogService.Close(createdTask);
                }
                else
                {
                    // Update existing task
                    await ApiService.PutAsync($"TaskItems/{Task.Id}", Task);
                    DialogService.Close(Task);
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to save task");
                Console.WriteLine($"Error saving task: {ex.Message}");
            }
        }

        #endregion
    }
}
