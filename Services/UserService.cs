using System.Security.Cryptography;
using System.Text;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Repositories;

namespace EVChargingBookingAPI.Services
{
    /// <summary>
    /// Service implementation for User business logic
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEVOwnerRepository _evOwnerRepository;

        public UserService(IUserRepository userRepository, IEVOwnerRepository evOwnerRepository)
        {
            _userRepository = userRepository;
            _evOwnerRepository = evOwnerRepository;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            // Validate role
            if (user.Role != "Backoffice" && user.Role != "StationOperator")
            {
                throw new ArgumentException("Invalid user role. Must be 'Backoffice' or 'StationOperator'");
            }

            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(user.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            // Hash password
            user.PasswordHash = HashPassword(password);

            return await _userRepository.CreateAsync(user);
        }

        public async Task<User> UpdateUserAsync(string id, User user)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
            {
                throw new ArgumentException("User not found");
            }

            // Preserve password hash
            user.PasswordHash = existingUser.PasswordHash;
            
            await _userRepository.UpdateAsync(id, user);
            return user;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            await _userRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            return VerifyPassword(password, user.PasswordHash);
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _userRepository.UpdateAsync(userId, user);
            return true;
        }

        public async Task<EVOwner> GetEVOwnerByNICAsync(string nic)
        {
            return await _evOwnerRepository.GetByNICAsync(nic);
        }

        public async Task<EVOwner> CreateEVOwnerAsync(EVOwner evOwner)
        {
            await _evOwnerRepository.CreateAsync(evOwner);
            return evOwner;
        }

        public async Task<List<User>> GetUsersByRoleAsync(string role)
        {
            return await _userRepository.GetByRoleAsync(role);
        }

        public async Task<User> CreateStationOperatorAsync(string username, string password, string email)
        {
            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                Role = "StationOperator",
                PasswordHash = HashPassword(password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            return await _userRepository.CreateAsync(user);
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = false;
            await _userRepository.UpdateAsync(userId, user);
            return true;
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsActive = true;
            await _userRepository.UpdateAsync(userId, user);
            return true;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }
    }
}