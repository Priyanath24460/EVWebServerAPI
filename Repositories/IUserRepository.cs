using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.Repositories
{
    /// <summary>
    /// Interface for User data operations
    /// </summary>
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User> GetByIdAsync(string id);
        Task<User> GetByUsernameAsync(string username);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(string id, User user);
        Task DeleteAsync(string id);
        Task<bool> UsernameExistsAsync(string username);
    }
}