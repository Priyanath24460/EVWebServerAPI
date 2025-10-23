using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Repositories;

namespace EVChargingBookingAPI.Services
{
    /// <summary>
    /// Service implementation for Booking business logic
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEVOwnerRepository _evOwnerRepository;

        public BookingService(IBookingRepository bookingRepository, IEVOwnerRepository evOwnerRepository)
        {
            _bookingRepository = bookingRepository;
            _evOwnerRepository = evOwnerRepository;
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<Booking> GetBookingByIdAsync(string id)
        {
            return await _bookingRepository.GetByIdAsync(id);
        }

        public async Task<List<Booking>> GetUpcomingBookingsByEVOwnerAsync(string nic)
        {
            // Check if EV Owner exists and is active
            var evOwner = await _evOwnerRepository.GetByNICAsync(nic);
            if (evOwner == null || !evOwner.IsActive)
            {
                throw new ArgumentException("EV Owner not found or inactive");
            }

            return await _bookingRepository.GetUpcomingByEVOwnerNICAsync(nic);
        }

        public async Task<List<Booking>> GetBookingHistoryByEVOwnerAsync(string nic)
        {
            var evOwner = await _evOwnerRepository.GetByNICAsync(nic);
            if (evOwner == null || !evOwner.IsActive)
            {
                throw new ArgumentException("EV Owner not found or inactive");
            }

            return await _bookingRepository.GetHistoryByEVOwnerNICAsync(nic);
        }

        public async Task<int> GetPendingBookingsCountAsync(string nic)
        {
            var evOwner = await _evOwnerRepository.GetByNICAsync(nic);
            if (evOwner == null || !evOwner.IsActive)
            {
                throw new ArgumentException("EV Owner not found or inactive");
            }

            return await _bookingRepository.GetPendingCountByEVOwnerNICAsync(nic);
        }

        public async Task<int> GetApprovedBookingsCountAsync(string nic)
        {
            var evOwner = await _evOwnerRepository.GetByNICAsync(nic);
            if (evOwner == null || !evOwner.IsActive)
            {
                throw new ArgumentException("EV Owner not found or inactive");
            }

            return await _bookingRepository.GetApprovedCountByEVOwnerNICAsync(nic);
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            // Business Logic: Reservation must be within 7 days
            if (!await IsReservationWithinAllowedPeriodAsync(booking.StartTime))
            {
                throw new InvalidOperationException("Reservation must be within 7 days from booking date");
            }

            // Check if EV Owner exists and is active
            var evOwner = await _evOwnerRepository.GetByNICAsync(booking.EVOwnerNIC);
            if (evOwner == null || !evOwner.IsActive)
            {
                throw new ArgumentException("EV Owner not found or inactive");
            }

            await _bookingRepository.CreateAsync(booking);
            return booking;
        }

        public async Task<Booking> UpdateBookingAsync(string id, Booking booking)
        {
            var existingBooking = await _bookingRepository.GetByIdAsync(id);
            if (existingBooking == null)
            {
                throw new ArgumentException("Booking not found");
            }

            // Business Logic: Can only update at least 12 hours before reservation
            if (!await CanModifyBookingAsync(existingBooking.StartTime))
            {
                throw new InvalidOperationException("Booking can only be modified at least 12 hours before reservation");
            }

            // Business Logic: If changing reservation time, must be within 7 days
            if (booking.StartTime != existingBooking.StartTime)
            {
                if (!await IsReservationWithinAllowedPeriodAsync(booking.StartTime))
                {
                    throw new InvalidOperationException("Reservation must be within 7 days from booking date");
                }
            }

            booking.Id = id;
            await _bookingRepository.UpdateAsync(id, booking);
            return booking;
        }

        public async Task<bool> CancelBookingAsync(string id)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                throw new ArgumentException("Booking not found");
            }

            // Business Logic: Can only cancel at least 12 hours before reservation
            if (!await CanModifyBookingAsync(booking.StartTime))
            {
                throw new InvalidOperationException("Booking can only be cancelled at least 12 hours before reservation");
            }

            // Delete the booking instead of updating status
            await _bookingRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> CanModifyBookingAsync(DateTime startTime)
        {
            var timeDifference = startTime - DateTime.UtcNow;
            bool canModify = timeDifference.TotalHours >= 12;
            return await Task.FromResult(canModify);
        }

        public async Task<bool> IsReservationWithinAllowedPeriodAsync(DateTime startTime)
        {
            var maxAllowedDate = DateTime.UtcNow.AddDays(7);
            bool isWithinPeriod = startTime <= maxAllowedDate && startTime >= DateTime.UtcNow;
            return await Task.FromResult(isWithinPeriod);
        }

        public async Task<List<Booking>> GetBookingsByStationIdAsync(string stationId)
        {
            return await _bookingRepository.GetByChargingStationIdAsync(stationId);
        }
    }
}