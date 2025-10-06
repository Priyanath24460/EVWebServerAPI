using MongoDB.Driver;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Data;

namespace EVChargingBookingAPI.Repositories
{
    /// <summary>
    /// Repository implementation for EV Owner operations
    /// </summary>
    public class EVOwnerRepository : IEVOwnerRepository
    {
        private readonly MongoDBContext _context;

        public EVOwnerRepository(MongoDBContext context)
        {
            _context = context;
        }

        public async Task<List<EVOwner>> GetAllAsync()
        {
            return await _context.EVOwners.Find(_ => true).ToListAsync();
        }

        public async Task<EVOwner> GetByNICAsync(string nic)
        {
            return await _context.EVOwners.Find(e => e.NIC == nic).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(EVOwner evOwner)
        {
            // Set timestamps
            evOwner.CreatedAt = DateTime.UtcNow;
            evOwner.UpdatedAt = DateTime.UtcNow;
            
            await _context.EVOwners.InsertOneAsync(evOwner);
        }

        public async Task UpdateAsync(string nic, EVOwner evOwner)
        {
            evOwner.UpdatedAt = DateTime.UtcNow;
            var filter = Builders<EVOwner>.Filter.Eq(e => e.NIC, nic);
            await _context.EVOwners.ReplaceOneAsync(filter, evOwner);
        }

        public async Task DeleteAsync(string nic)
        {
            var filter = Builders<EVOwner>.Filter.Eq(e => e.NIC, nic);
            await _context.EVOwners.DeleteOneAsync(filter);
        }

        public async Task<bool> ExistsAsync(string nic)
        {
            var filter = Builders<EVOwner>.Filter.Eq(e => e.NIC, nic);
            return await _context.EVOwners.Find(filter).AnyAsync();
        }
    }
}