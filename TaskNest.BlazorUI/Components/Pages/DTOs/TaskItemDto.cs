namespace TaskNest.BlazorUI.Components.Pages.DTOs
{
    public class TaskItemDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [MinLength(2, ErrorMessage = "Task title must be at least 2 characters long")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Task description is required")]
        [MinLength(5, ErrorMessage = "Task description must be at least 5 characters long")]
        public string Description { get; set; }

        [Required]
        public Guid ColumnId { get; set; }

        [Required]
        public int Position { get; set; }

        [Required]
        public Guid BoardId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

    }
}
