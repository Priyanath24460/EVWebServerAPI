using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.Services
{
    /// <summary>
    /// Interface for User business logic
    /// </summary>
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(string id);
        Task<User> GetUserByUsernameAsync(string username);
        Task<User> CreateUserAsync(User user, string password);
        Task<User> UpdateUserAsync(string id, User user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> AuthenticateUserAsync(string username, string password);
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        
        // Role-based methods
        Task<List<User>> GetUsersByRoleAsync(string role);
        Task<User> CreateStationOperatorAsync(string username, string password, string email);
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> ActivateUserAsync(string userId);
        
        // EV Owner methods
        Task<EVOwner> GetEVOwnerByNICAsync(string nic);
        Task<EVOwner> CreateEVOwnerAsync(EVOwner evOwner);
    }
}