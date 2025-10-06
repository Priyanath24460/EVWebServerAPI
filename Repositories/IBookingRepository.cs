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
        Task CreateAsync(Booking booking);
        Task UpdateAsync(string id, Booking booking);
        Task DeleteAsync(string id);
        Task<bool> HasActiveBookingsForStationAsync(string stationId);
    }
}