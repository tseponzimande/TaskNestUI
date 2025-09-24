namespace TaskNest.BlazorUI.Components.Pages.DTOs
{
    public class AddBoardUserDto
    {
        [Required(ErrorMessage = "User email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; }

    }
}
