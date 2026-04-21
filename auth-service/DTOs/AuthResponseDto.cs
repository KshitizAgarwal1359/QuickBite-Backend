namespace QuickBite.Auth.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public ProfileResponseDto User { get; set; } = null!;
    }
}
