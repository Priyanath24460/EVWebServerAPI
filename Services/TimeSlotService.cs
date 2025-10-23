using EVChargingBookingAPI.DTOs;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Repositories;

namespace EVChargingBookingAPI.Services
{
    /// <summary>
    /// Service for managing time slots and availability
    /// </summary>
    public interface ITimeSlotService
    {
        Task<StationAvailabilityDTO> GetStationAvailabilityAsync(string stationId, DateTime date);
        Task<MobileStationAvailabilityDTO> GetMobileStationAvailabilityAsync(string stationId, DateTime date);
        Task<List<ChargingPointSlotsDTO>> GetAvailableTimeSlotsAsync(string stationId, DateTime date);
        Task<bool> IsTimeSlotAvailableAsync(string stationId, int chargingPointNumber, DateTime date, int timeSlot);
        Task<Booking> CreateTimeSlotBookingAsync(CreateTimeSlotBookingDTO bookingDto);
    }

    public class TimeSlotService : ITimeSlotService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IChargingStationRepository _stationRepository;
        private const int CHARGING_POINTS_PER_STATION = 3;
        private const int HOURS_PER_DAY = 24;

        public TimeSlotService(IBookingRepository bookingRepository, IChargingStationRepository stationRepository)
        {
            _bookingRepository = bookingRepository;
            _stationRepository = stationRepository;
        }

        public async Task<StationAvailabilityDTO> GetStationAvailabilityAsync(string stationId, DateTime date)
        {
            var station = await _stationRepository.GetByIdAsync(stationId);
            if (station == null)
                throw new ArgumentException("Charging station not found");

            var bookingDate = date.Date;
            var existingBookings = await _bookingRepository.GetBookingsByStationAndDateAsync(stationId, bookingDate);

            var stationAvailability = new StationAvailabilityDTO
            {
                StationId = stationId,
                StationName = station.Name,
                Date = bookingDate,
                ChargingPoints = new List<ChargingPointSlotsDTO>()
            };

            // Generate time slots for each charging point
            for (int pointNumber = 1; pointNumber <= CHARGING_POINTS_PER_STATION; pointNumber++)
            {
                var chargingPointSlots = new ChargingPointSlotsDTO
                {
                    ChargingPointNumber = pointNumber,
                    TimeSlots = new List<TimeSlotDTO>()
                };

                // Generate 24 hourly slots for this charging point
                for (int hour = 0; hour < HOURS_PER_DAY; hour++)
                {
                    var existingBooking = existingBookings.FirstOrDefault(b => 
                        b.ChargingPointNumber == pointNumber && b.TimeSlot == hour);

                    var timeSlot = new TimeSlotDTO
                    {
                        Hour = hour,
                        TimeRange = $"{hour:D2}:00 - {(hour + 1):D2}:00",
                        IsAvailable = existingBooking == null,
                        BookedBy = existingBooking?.EVOwnerNIC,
                        BookingId = existingBooking?.Id
                    };

                    chargingPointSlots.TimeSlots.Add(timeSlot);
                }

                stationAvailability.ChargingPoints.Add(chargingPointSlots);
            }

            return stationAvailability;
        }

        public async Task<MobileStationAvailabilityDTO> GetMobileStationAvailabilityAsync(string stationId, DateTime date)
        {
            var station = await _stationRepository.GetByIdAsync(stationId);
            if (station == null)
                throw new ArgumentException("Charging station not found");

            var bookingDate = date.Date;
            var existingBookings = await _bookingRepository.GetBookingsByStationAndDateAsync(stationId, bookingDate);

            var mobileAvailability = new MobileStationAvailabilityDTO
            {
                stationId = stationId,
                date = bookingDate.ToString("yyyy-MM-dd"),
                chargingPoints = new List<MobileChargingPointSlotsDTO>()
            };

            // Generate time slots for each charging point
            for (int pointNumber = 1; pointNumber <= CHARGING_POINTS_PER_STATION; pointNumber++)
            {
                var mobileChargingPointSlots = new MobileChargingPointSlotsDTO
                {
                    chargingPointNumber = pointNumber,
                    timeSlots = new List<MobileTimeSlotDTO>()
                };

                // Generate 24 hourly slots for this charging point
                for (int hour = 0; hour < HOURS_PER_DAY; hour++)
                {
                    var existingBooking = existingBookings.FirstOrDefault(b => 
                        b.ChargingPointNumber == pointNumber && b.TimeSlot == hour);

                    var mobileTimeSlot = new MobileTimeSlotDTO
                    {
                        hour = hour,
                        displayTime = $"{hour:D2}:00",
                        isAvailable = existingBooking == null
                    };

                    mobileChargingPointSlots.timeSlots.Add(mobileTimeSlot);
                }

                mobileAvailability.chargingPoints.Add(mobileChargingPointSlots);
            }

            return mobileAvailability;
        }

        public async Task<List<ChargingPointSlotsDTO>> GetAvailableTimeSlotsAsync(string stationId, DateTime date)
        {
            var stationAvailability = await GetStationAvailabilityAsync(stationId, date);
            
            // Filter to only show available slots
            var availableSlots = new List<ChargingPointSlotsDTO>();
            
            foreach (var chargingPoint in stationAvailability.ChargingPoints)
            {
                var availableChargingPoint = new ChargingPointSlotsDTO
                {
                    ChargingPointNumber = chargingPoint.ChargingPointNumber,
                    TimeSlots = chargingPoint.TimeSlots.Where(ts => ts.IsAvailable).ToList()
                };
                
                availableSlots.Add(availableChargingPoint);
            }

            return availableSlots;
        }

        public async Task<bool> IsTimeSlotAvailableAsync(string stationId, int chargingPointNumber, DateTime date, int timeSlot)
        {
            if (chargingPointNumber < 1 || chargingPointNumber > CHARGING_POINTS_PER_STATION)
                return false;

            if (timeSlot < 0 || timeSlot >= HOURS_PER_DAY)
                return false;

            return await _bookingRepository.IsTimeSlotAvailableAsync(stationId, chargingPointNumber, date, timeSlot);
        }

        public async Task<Booking> CreateTimeSlotBookingAsync(CreateTimeSlotBookingDTO bookingDto)
        {
            // Validate input
            if (bookingDto.ChargingPointNumber < 1 || bookingDto.ChargingPointNumber > CHARGING_POINTS_PER_STATION)
                throw new ArgumentException("Invalid charging point number. Must be 1, 2, or 3.");

            if (bookingDto.TimeSlot < 0 || bookingDto.TimeSlot >= HOURS_PER_DAY)
                throw new ArgumentException("Invalid time slot. Must be between 0 and 23.");

            var bookingDate = bookingDto.BookingDate.Date;

            // Check if the time slot is available
            var isAvailable = await IsTimeSlotAvailableAsync(
                bookingDto.ChargingStationId, 
                bookingDto.ChargingPointNumber, 
                bookingDate, 
                bookingDto.TimeSlot);

            if (!isAvailable)
                throw new InvalidOperationException("Time slot is not available.");

            // Create the booking
            var startTime = bookingDate.AddHours(bookingDto.TimeSlot);
            var endTime = startTime.AddHours(1);

            var booking = new Booking
            {
                EVOwnerNIC = bookingDto.EVOwnerNIC,
                ChargingStationId = bookingDto.ChargingStationId,
                ChargingPointNumber = bookingDto.ChargingPointNumber,
                BookingDate = bookingDate,
                TimeSlot = bookingDto.TimeSlot,
                StartTime = startTime,
                EndTime = endTime,
                DurationMinutes = 60,
                Status = "Approved", // Auto-approve for time slot bookings
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _bookingRepository.CreateAsync(booking);
            return booking;
        }
    }
}