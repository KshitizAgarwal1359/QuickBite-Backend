namespace QuickBite.Auth.DTOs
{
    public class ProfileResponseDto
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string Role { get; set; } = string.Empty;

        public string? ProfilePicUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
