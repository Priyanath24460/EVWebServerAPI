using Microsoft.AspNetCore.Mvc;
using EVChargingBookingAPI.Services;
using EVChargingBookingAPI.DTOs;

namespace EVChargingBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeSlotsController : ControllerBase
    {
        private readonly ITimeSlotService _timeSlotService;

        public TimeSlotsController(ITimeSlotService timeSlotService)
        {
            _timeSlotService = timeSlotService;
        }

        /// <summary>
        /// Get availability for all charging points at a station on a specific date
        /// </summary>
        /// <param name="stationId">Charging station ID</param>
        /// <param name="date">Date to check (YYYY-MM-DD)</param>
        /// <returns>Complete availability information with all time slots</returns>
        [HttpGet("availability/{stationId}")]
        public async Task<ActionResult<MobileStationAvailabilityDTO>> GetStationAvailability(
            string stationId, 
            [FromQuery] DateTime date)
        {
            try
            {
                var availability = await _timeSlotService.GetMobileStationAvailabilityAsync(stationId, date);
                return Ok(availability);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get station availability", detail = ex.Message });
            }
        }

        /// <summary>
        /// Get only available time slots for a station on a specific date
        /// </summary>
        /// <param name="stationId">Charging station ID</param>
        /// <param name="date">Date to check (YYYY-MM-DD)</param>
        /// <returns>Only available time slots (no booked slots)</returns>
        [HttpGet("station/{stationId}/available")]
        public async Task<ActionResult<List<ChargingPointSlotsDTO>>> GetAvailableTimeSlots(
            string stationId, 
            [FromQuery] DateTime date)
        {
            try
            {
                var availableSlots = await _timeSlotService.GetAvailableTimeSlotsAsync(stationId, date);
                return Ok(availableSlots);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get available time slots", detail = ex.Message });
            }
        }

        /// <summary>
        /// Check if a specific time slot is available
        /// </summary>
        /// <param name="stationId">Charging station ID</param>
        /// <param name="chargingPointNumber">Charging point number (1-3)</param>
        /// <param name="date">Date to check</param>
        /// <param name="timeSlot">Hour of the day (0-23)</param>
        /// <returns>True if available, false if booked</returns>
        [HttpGet("station/{stationId}/point/{chargingPointNumber}/check")]
        public async Task<ActionResult<bool>> CheckTimeSlotAvailability(
            string stationId,
            int chargingPointNumber,
            [FromQuery] DateTime date,
            [FromQuery] int timeSlot)
        {
            try
            {
                var isAvailable = await _timeSlotService.IsTimeSlotAvailableAsync(
                    stationId, chargingPointNumber, date, timeSlot);
                return Ok(new { isAvailable });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to check time slot availability", detail = ex.Message });
            }
        }

        /// <summary>
        /// Create a new time slot booking
        /// </summary>
        /// <param name="bookingDto">Booking details</param>
        /// <returns>Booking response with QR code</returns>
        [HttpPost("book")]
        public async Task<ActionResult<BookingResponseDTO>> CreateTimeSlotBooking([FromBody] CreateTimeSlotBookingDTO bookingDto)
        {
            try
            {
                var booking = await _timeSlotService.CreateTimeSlotBookingAsync(bookingDto);
                
                var response = new BookingResponseDTO
                {
                    BookingId = booking.Id,
                    Status = booking.Status,
                    Message = "Booking created successfully",
                    QrCodeData = booking.QRCodeData
                };
                
                return CreatedAtAction(
                    nameof(BookingsController.GetById), 
                    "Bookings", 
                    new { id = booking.Id }, 
                    response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create booking", detail = ex.Message });
            }
        }
    }
}