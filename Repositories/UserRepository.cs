using MongoDB.Driver;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Data;

namespace EVChargingBookingAPI.Repositories
{
    /// <summary>
    /// Repository implementation for User operations
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly MongoDBContext _context;

        public UserRepository(MongoDBContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.Find(_ => true).ToListAsync();
        }

        public async Task<User> GetByIdAsync(string id)
        {
            return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.InsertOneAsync(user);
            return user;
        }

        public async Task UpdateAsync(string id, User user)
        {
            await _context.Users.ReplaceOneAsync(u => u.Id == id, user);
        }

        public async Task DeleteAsync(string id)
        {
            await _context.Users.DeleteOneAsync(u => u.Id == id);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.Find(u => u.Username == username).AnyAsync();
        }
    }
}