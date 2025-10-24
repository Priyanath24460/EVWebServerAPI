using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.Services
{
    /// <summary>
    /// Interface for Booking business logic
    /// </summary>
    public interface IBookingService
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking> GetBookingByIdAsync(string id);
        Task<List<Booking>> GetUpcomingBookingsByEVOwnerAsync(string nic);
        Task<List<Booking>> GetBookingHistoryByEVOwnerAsync(string nic);
        Task<int> GetPendingBookingsCountAsync(string nic);
        Task<int> GetApprovedBookingsCountAsync(string nic);
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<Booking> UpdateBookingAsync(string id, Booking booking);
        Task<bool> CancelBookingAsync(string id);
        Task<bool> CanModifyBookingAsync(DateTime startTime);
        Task<bool> IsReservationWithinAllowedPeriodAsync(DateTime startTime);
        Task<List<Booking>> GetBookingsByStationIdAsync(string stationId);
        Task<List<Booking>> GetActiveBookingsByStationIdAsync(string stationId);
        Task<List<Booking>> GetBookingsByOperatorUsernameAsync(string operatorUsername);
    }
}