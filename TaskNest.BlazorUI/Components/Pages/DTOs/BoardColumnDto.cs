namespace TaskNest.BlazorUI.Components.Pages.DTOs
{
    public class BoardColumnDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Column name is required")]
        [MinLength(2, ErrorMessage = "Column name must be at least 2 characters long")]
        public string Name { get; set; }

        [Required]
        public int Order { get; set; }

        [Required]
        public Guid BoardId { get; set; }

        [Required]
        public List<TaskItemDto> Tasks { get; set; } = new();
    }
}
