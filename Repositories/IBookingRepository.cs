using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.Repositories
{
    /// <summary>
    /// Interface for Booking data operations
    /// </summary>
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllAsync();
        Task<Booking> GetByIdAsync(string id);
        Task<List<Booking>> GetByEVOwnerNICAsync(string nic);
        Task<List<Booking>> GetUpcomingByEVOwnerNICAsync(string nic);
        Task<List<Booking>> GetHistoryByEVOwnerNICAsync(string nic);
        Task<int> GetPendingCountByEVOwnerNICAsync(string nic);
        Task<int> GetApprovedCountByEVOwnerNICAsync(string nic);
        Task CreateAsync(Booking booking);
        Task UpdateAsync(string id, Booking booking);
        Task DeleteAsync(string id);
        Task<bool> HasActiveBookingsForStationAsync(string stationId);
        Task<List<Booking>> GetByChargingStationIdAsync(string stationId);
        
        // Time slot specific methods
        Task<List<Booking>> GetBookingsByStationAndDateAsync(string stationId, DateTime date);
        Task<Booking?> GetBookingByTimeSlotAsync(string stationId, int chargingPointNumber, DateTime date, int timeSlot);
        Task<bool> IsTimeSlotAvailableAsync(string stationId, int chargingPointNumber, DateTime date, int timeSlot);
    }
}