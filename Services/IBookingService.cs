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
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<Booking> UpdateBookingAsync(string id, Booking booking);
        Task<bool> CancelBookingAsync(string id);
        Task<bool> CanModifyBookingAsync(DateTime reservationDateTime);
        Task<bool> IsReservationWithinAllowedPeriodAsync(DateTime reservationDateTime);
    }
}