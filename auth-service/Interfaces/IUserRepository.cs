using QuickBite.Auth.Entities;

namespace QuickBite.Auth.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByUserIdAsync(int userId);
        Task<bool> ExistsByEmailAsync(string email);
        Task<List<User>> FindAllByRoleAsync(string role);
        Task<User?> FindByPhoneAsync(string phone);
        Task<List<User>> FindByFullNameContainingAsync(string nameFragment);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteByUserIdAsync(int userId);
    }
}
