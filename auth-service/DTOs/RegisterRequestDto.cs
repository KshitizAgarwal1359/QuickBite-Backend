using System.ComponentModel.DataAnnotations;

namespace QuickBite.Auth.DTOs
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(CUSTOMER|OWNER|AGENT|ADMIN)$", ErrorMessage = "Role must be CUSTOMER, OWNER, AGENT, or ADMIN")]
        public string Role { get; set; } = "CUSTOMER";
    }
}
