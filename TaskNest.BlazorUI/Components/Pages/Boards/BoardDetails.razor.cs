using Microsoft.JSInterop;

namespace TaskNest.BlazorUI.Components.Pages.Boards
{
    [Authorize]
    public partial class BoardDetails
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

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public ILogger Logger { get; set; }

        #endregion

        #region Parameters and Fields

        [Parameter]
        public Guid Id { get; set; }

        #endregion

        #region Properties

        private BoardDto board;

        private List<BoardColumnDto> columns;

        private Dictionary<Guid, List<TaskItemDto>> tasksByColumn = new();

        // Search and Filter Properties
        private string searchTerm = "";
        private string selectedFilter = "all";
        private List<dynamic> filterOptions = new List<dynamic>
        {
            new { Text = "All Tasks", Value = "all" },
            new { Text = "Overdue", Value = "overdue" },
            new { Text = "Due Today", Value = "today" },
            new { Text = "Due This Week", Value = "week" },
            new { Text = "No Due Date", Value = "nodate" }
        };

        // Original task data before filtering
        private Dictionary<Guid, List<TaskItemDto>> originalTasksByColumn = new();

        private bool loading = true;

        private IJSObjectReference _dragDropModule;

        #endregion

        #region Methods

        protected override async Task OnInitializedAsync()
        {
            await LoadBoardData();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // Initialize drag and drop
                try
                {
                    _dragDropModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/dragdrop.js");
                    await _dragDropModule.InvokeVoidAsync("initializeDragDrop", DotNetObjectReference.Create(this));
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error initializing drag and drop");

                    //Console.WriteLine($"Error initializing drag and drop: {ex.Message}");
                }
            }
        }

        private async Task LoadBoardData()
        {
            try
            {
                loading = true;
                board = await ApiService.GetAsync<BoardDto>($"Boards/{Id}");
                columns = (await ApiService.GetAsync<List<BoardColumnDto>>($"BoardColumns/byBoard/{Id}")).ToList();

                // Load tasks for each column
                originalTasksByColumn.Clear();
                tasksByColumn.Clear();

                foreach (var column in columns)
                {
                    var tasks = await ApiService.GetAsync<List<TaskItemDto>>($"TaskItems/byColumn/{column.Id}");
                    originalTasksByColumn[column.Id] = tasks.ToList();
                    tasksByColumn[column.Id] = tasks.ToList();
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to load board data");
                Logger.LogError(ex, "Error loading board data");
                //Console.WriteLine($"Error loading board data: {ex.Message}");
            }
            finally
            {
                loading = false;
            }
        }

        private void FilterTasks()
        {
            // Reset to original data first
            tasksByColumn = new Dictionary<Guid, List<TaskItemDto>>();
            foreach (var kvp in originalTasksByColumn)
            {
                tasksByColumn[kvp.Key] = new List<TaskItemDto>(kvp.Value);
            }

            // Apply search term filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                foreach (var columnId in tasksByColumn.Keys.ToList())
                {
                    tasksByColumn[columnId] = tasksByColumn[columnId]
                        .Where(t => t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                   (t.Description != null && t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }
            }

            // Apply date filter
            if (selectedFilter != "all")
            {
                foreach (var columnId in tasksByColumn.Keys.ToList())
                {
                    tasksByColumn[columnId] = selectedFilter switch
                    {
                        "overdue" => tasksByColumn[columnId].Where(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.Today).ToList(),
                        "today" => tasksByColumn[columnId].Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today).ToList(),
                        "week" => tasksByColumn[columnId].Where(t => t.DueDate.HasValue &&
                                                                  t.DueDate.Value.Date >= DateTime.Today &&
                                                                  t.DueDate.Value.Date <= DateTime.Today.AddDays(7)).ToList(),
                        "nodate" => tasksByColumn[columnId].Where(t => !t.DueDate.HasValue).ToList(),
                        _ => tasksByColumn[columnId]
                    };
                }
            }

            StateHasChanged();
        }

        private void ResetFilters()
        {
            searchTerm = "";
            selectedFilter = "all";

            // Reset to original data
            tasksByColumn = new Dictionary<Guid, List<TaskItemDto>>();
            foreach (var kvp in originalTasksByColumn)
            {
                tasksByColumn[kvp.Key] = new List<TaskItemDto>(kvp.Value);
            }

            StateHasChanged();
        }

        private async Task ShowAddColumnDialog()
        {
            var newColumn = new BoardColumnDto { BoardId = Id };
            var result = await DialogService.OpenAsync<ColumnDialog>("Add Column",
                new Dictionary<string, object> { { "Column", newColumn } },
                new DialogOptions { Width = "400px", Height = "auto", CloseDialogOnOverlayClick = true });

            if (result != null)
            {
                await LoadBoardData();
                NotificationService.Notify(NotificationSeverity.Success, "Success", "Column added successfully");
            }
        }

        private async Task EditColumn(BoardColumnDto column)
        {
            var result = await DialogService.OpenAsync<ColumnDialog>("Edit Column",
                new Dictionary<string, object> { { "Column", column } },
                new DialogOptions { Width = "400px", Height = "auto", CloseDialogOnOverlayClick = true });

            if (result != null)
            {
                await LoadBoardData();
                NotificationService.Notify(NotificationSeverity.Success, "Success", "Column updated successfully");
            }
        }

        private async Task ConfirmDeleteColumn(Guid columnId)
        {
            var result = await DialogService.Confirm("Are you sure you want to delete this column?", "Delete Column",
                new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" });

            if (result == true)
            {
                try
                {
                    await ApiService.DeleteAsync($"BoardColumns/{columnId}");
                    await LoadBoardData();
                    NotificationService.Notify(NotificationSeverity.Success, "Success", "Column deleted successfully");
                }
                catch (Exception ex)
                {
                    NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to delete column");
                    Logger.LogError(ex, "Error deleting column");
                    //Console.WriteLine($"Error deleting column: {ex.Message}");
                }
            }
        }

        private async Task ShowAddTaskDialog(Guid columnId)
        {
            var newTask = new TaskItemDto { BoardId = Id, ColumnId = columnId };
            var result = await DialogService.OpenAsync<TaskDialog>("Add Task",
                new Dictionary<string, object> {
                { "Task", newTask },
                { "Columns", columns }
                    },
                new DialogOptions { Width = "500px", Height = "auto", CloseDialogOnOverlayClick = true });

            if (result != null)
            {
                await LoadBoardData();
                NotificationService.Notify(NotificationSeverity.Success, "Success", "Task added successfully");
            }
        }

        private async Task EditTask(TaskItemDto task)
        {
            var result = await DialogService.OpenAsync<TaskDialog>("Edit Task",
                new Dictionary<string, object> {
                { "Task", task },
                { "Columns", columns }
                    },
                new DialogOptions { Width = "500px", Height = "auto", CloseDialogOnOverlayClick = true });

            if (result != null)
            {
                await LoadBoardData();
                NotificationService.Notify(NotificationSeverity.Success, "Success", "Task updated successfully");
            }
        }

        [JSInvokable]
        public async Task TaskMoved(string taskId, string sourceColumnId, string targetColumnId, int newIndex)
        {
            try
            {
                var task = await ApiService.GetAsync<TaskItemDto>($"TaskItems/{taskId}");
                task.ColumnId = Guid.Parse(targetColumnId);
                task.Position = newIndex;

                await ApiService.PutAsync($"TaskItems/{taskId}", task);

                // Update both original and filtered collections
                if (originalTasksByColumn.ContainsKey(Guid.Parse(sourceColumnId)))
                {
                    var movedTask = originalTasksByColumn[Guid.Parse(sourceColumnId)].FirstOrDefault(t => t.Id.ToString() == taskId);
                    if (movedTask != null)
                    {
                        originalTasksByColumn[Guid.Parse(sourceColumnId)].Remove(movedTask);
                        movedTask.ColumnId = Guid.Parse(targetColumnId);

                        if (!originalTasksByColumn.ContainsKey(Guid.Parse(targetColumnId)))
                        {
                            originalTasksByColumn[Guid.Parse(targetColumnId)] = new List<TaskItemDto>();
                        }

                        originalTasksByColumn[Guid.Parse(targetColumnId)].Insert(newIndex, movedTask);
                    }
                }

                // Also update the filtered collection
                if (tasksByColumn.ContainsKey(Guid.Parse(sourceColumnId)))
                {
                    var movedTask = tasksByColumn[Guid.Parse(sourceColumnId)].FirstOrDefault(t => t.Id.ToString() == taskId);
                    if (movedTask != null)
                    {
                        tasksByColumn[Guid.Parse(sourceColumnId)].Remove(movedTask);
                        movedTask.ColumnId = Guid.Parse(targetColumnId);

                        if (!tasksByColumn.ContainsKey(Guid.Parse(targetColumnId)))
                        {
                            tasksByColumn[Guid.Parse(targetColumnId)] = new List<TaskItemDto>();
                        }

                        tasksByColumn[Guid.Parse(targetColumnId)].Insert(newIndex, movedTask);
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(NotificationSeverity.Error, "Error", "Failed to update task position");
                //Console.WriteLine($"Error moving task: {ex.Message}");
                Logger.LogError(ex, "Error moving task");
                await LoadBoardData(); // Reload to restore correct state
            }
        }

        private async Task ShowBoardUsersDialog()
        {
            NavigationManager.NavigateTo($"/boards/{Id}/users");
        }

        #endregion
    }
}
