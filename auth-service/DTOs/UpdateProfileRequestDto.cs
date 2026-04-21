using System.ComponentModel.DataAnnotations;

namespace QuickBite.Auth.DTOs
{
    public class UpdateProfileRequestDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15)]
        public string? Phone { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        public string? ProfilePicUrl { get; set; }
    }
}
