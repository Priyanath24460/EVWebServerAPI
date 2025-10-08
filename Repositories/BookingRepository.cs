using MongoDB.Driver;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Data;

namespace EVChargingBookingAPI.Repositories
{
    /// <summary>
    /// Repository implementation for Booking operations
    /// </summary>
    public class BookingRepository : IBookingRepository
    {
        private readonly MongoDBContext _context;

        public BookingRepository(MongoDBContext context)
        {
            _context = context;
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            return await _context.Bookings.Find(_ => true).ToListAsync();
        }

        public async Task<Booking> GetByIdAsync(string id)
        {
            return await _context.Bookings.Find(b => b.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Booking>> GetByEVOwnerNICAsync(string nic)
        {
            return await _context.Bookings.Find(b => b.EVOwnerNIC == nic).ToListAsync();
        }

        public async Task<List<Booking>> GetUpcomingByEVOwnerNICAsync(string nic)
        {
            var activeStatuses = new[] { "Pending", "Approved" };
            return await _context.Bookings.Find(b => b.EVOwnerNIC == nic && 
                b.ReservationDateTime >= DateTime.UtcNow && 
                activeStatuses.Contains(b.Status))
                .SortBy(b => b.ReservationDateTime)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetHistoryByEVOwnerNICAsync(string nic)
        {
            var historyStatuses = new[] { "Completed", "Cancelled" };
            return await _context.Bookings.Find(b => b.EVOwnerNIC == nic && 
                (b.ReservationDateTime < DateTime.UtcNow || historyStatuses.Contains(b.Status)))
                .SortByDescending(b => b.ReservationDateTime)
                .ToListAsync();
        }

        public async Task<int> GetPendingCountByEVOwnerNICAsync(string nic)
        {
            return (int)await _context.Bookings.CountDocumentsAsync(b => b.EVOwnerNIC == nic && b.Status == "Pending");
        }

        public async Task<int> GetApprovedCountByEVOwnerNICAsync(string nic)
        {
            return (int)await _context.Bookings.CountDocumentsAsync(b => b.EVOwnerNIC == nic && b.Status == "Approved");
        }

        public async Task CreateAsync(Booking booking)
        {
            // Generate unique booking reference
            booking.BookingReference = $"EVB{DateTime.UtcNow:yyyyMMddHHmmss}";
            await _context.Bookings.InsertOneAsync(booking);
        }

        public async Task UpdateAsync(string id, Booking booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            await _context.Bookings.ReplaceOneAsync(b => b.Id == id, booking);
        }

        public async Task DeleteAsync(string id)
        {
            await _context.Bookings.DeleteOneAsync(b => b.Id == id);
        }

        public async Task<bool> HasActiveBookingsForStationAsync(string stationId)
        {
            var activeStatuses = new[] { "Pending", "Approved" };
            return await _context.Bookings
                .Find(b => b.ChargingStationId == stationId && activeStatuses.Contains(b.Status))
                .AnyAsync();
        }
    }
}