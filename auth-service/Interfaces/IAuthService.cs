using QuickBite.Auth.DTOs;

namespace QuickBite.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<ProfileResponseDto> GetProfileAsync(int userId);
        Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileRequestDto request);
        Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request);
        Task DeactivateAccountAsync(int userId);
    }
}
