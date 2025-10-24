using Microsoft.AspNetCore.Mvc;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Services;
using EVChargingBookingAPI.DTOs;

namespace EVChargingBookingAPI.Controllers
{
    /// <summary>
    /// Controller for managing bookings
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IQRCodeService _qrCodeService;

        public BookingsController(IBookingService bookingService, IQRCodeService qrCodeService)
        {
            _bookingService = bookingService;
            _qrCodeService = qrCodeService;
        }

        /// <summary>
        /// Get all bookings
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Booking>>> GetAll()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get booking by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetById(string id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"Booking with ID {id} not found");
                }
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get upcoming bookings for an EV owner
        /// </summary>
        [HttpGet("upcoming/{nic}")]
        public async Task<ActionResult<List<Booking>>> GetUpcomingByEVOwner(string nic)
        {
            try
            {
                var bookings = await _bookingService.GetUpcomingBookingsByEVOwnerAsync(nic);
                return Ok(bookings);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get booking history for an EV owner
        /// </summary>
        [HttpGet("history/{nic}")]
        public async Task<ActionResult<List<Booking>>> GetBookingHistory(string nic)
        {
            try
            {
                var bookings = await _bookingService.GetBookingHistoryByEVOwnerAsync(nic);
                return Ok(bookings);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all bookings for a specific charging station (for station owners)
        /// </summary>
        [HttpGet("station/{stationId}")]
        public async Task<ActionResult<List<Booking>>> GetBookingsByStation(string stationId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByStationIdAsync(stationId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get active bookings for a specific charging station (for station owners)
        /// </summary>
        [HttpGet("station/{stationId}/active")]
        public async Task<ActionResult<List<Booking>>> GetActiveBookingsByStation(string stationId)
        {
            try
            {
                var bookings = await _bookingService.GetActiveBookingsByStationIdAsync(stationId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all bookings for a station operator's assigned station
        /// </summary>
        [HttpGet("operator/{operatorUsername}")]
        public async Task<ActionResult<List<Booking>>> GetBookingsByOperator(string operatorUsername)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByOperatorUsernameAsync(operatorUsername);
                return Ok(bookings);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get pending bookings count for an EV owner
        /// </summary>
        [HttpGet("pending/count/{nic}")]
        public async Task<ActionResult<int>> GetPendingBookingsCount(string nic)
        {
            try
            {
                var count = await _bookingService.GetPendingBookingsCountAsync(nic);
                return Ok(count);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get approved bookings count for an EV owner
        /// </summary>
        [HttpGet("approved/count/{nic}")]
        public async Task<ActionResult<int>> GetApprovedBookingsCount(string nic)
        {
            try
            {
                var count = await _bookingService.GetApprovedBookingsCountAsync(nic);
                return Ok(count);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new booking
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Booking>> Create(Booking booking)
        {
            try
            {
                var createdBooking = await _bookingService.CreateBookingAsync(booking);
                return CreatedAtAction(nameof(GetById), new { id = createdBooking.Id }, createdBooking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing booking
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Booking>> Update(string id, Booking booking)
        {
            try
            {
                if (id != booking.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var updatedBooking = await _bookingService.UpdateBookingAsync(id, booking);
                return Ok(updatedBooking);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel a booking
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Cancel(string id)
        {
            try
            {
                var result = await _bookingService.CancelBookingAsync(id);
                if (result)
                {
                    return Ok("Booking cancelled successfully");
                }
                return BadRequest("Failed to cancel booking");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a booking can be modified
        /// </summary>
        [HttpGet("{id}/can-modify")]
        public async Task<ActionResult<bool>> CanModify(string id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"Booking with ID {id} not found");
                }

                var canModify = await _bookingService.CanModifyBookingAsync(booking.StartTime);
                return Ok(canModify);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate QR code for approved booking
        /// </summary>
        [HttpPost("{id}/generate-qr")]
        public async Task<ActionResult<string>> GenerateQRCode(string id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"Booking with ID {id} not found");
                }

                var qrData = await _qrCodeService.GenerateQRCodeDataAsync(booking);
                
                // Update booking with QR code data
                booking.QRCodeData = qrData;
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingService.UpdateBookingAsync(id, booking);

                return Ok(new { qrData, message = "QR Code generated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validate QR code and return booking details
        /// </summary>
        [HttpPost("validate-qr")]
        public async Task<ActionResult<QRValidationResponse>> ValidateQRCode(QRValidationRequest request)
        {
            try
            {
                var qrData = await _qrCodeService.ValidateQRCodeAsync(request.QRData);
                var booking = await _bookingService.GetBookingByIdAsync(qrData.BookingId);

                var response = new QRValidationResponse
                {
                    IsValid = true,
                    BookingId = qrData.BookingId,
                    BookingReference = qrData.BookingReference,
                    EVOwnerNIC = qrData.EVOwnerNIC,
                    ChargingStationId = qrData.ChargingStationId,
                    ChargingPointNumber = qrData.ChargingPointNumber,
                    StartTime = qrData.StartTime,
                    DurationMinutes = qrData.DurationMinutes,
                    Status = booking?.Status ?? "Unknown"
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Ok(new QRValidationResponse 
                { 
                    IsValid = false, 
                    ErrorMessage = ex.Message 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update booking status (for operators - bypasses time restrictions)
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<Booking>> UpdateBookingStatus(string id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"Booking with ID {id} not found");
                }

                // Update status without time restrictions (for operator use)
                booking.Status = request.Status;
                booking.UpdatedAt = DateTime.UtcNow;

                // Direct repository update to bypass business rules
                await _bookingService.UpdateBookingStatusDirectAsync(id, booking);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Approve booking (for operators)
        /// </summary>
        [HttpPut("{id}/approve")]
        public async Task<ActionResult<Booking>> ApproveBooking(string id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"Booking with ID {id} not found");
                }

                booking.Status = "Approved";
                booking.UpdatedAt = DateTime.UtcNow;

                await _bookingService.UpdateBookingStatusDirectAsync(id, booking);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Start charging (for operators)
        /// </summary>
        [HttpPut("{id}/start")]
        public async Task<ActionResult<Booking>> StartCharging(string id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"Booking with ID {id} not found");
                }

                booking.Status = "Started";
                booking.UpdatedAt = DateTime.UtcNow;

                await _bookingService.UpdateBookingStatusDirectAsync(id, booking);
                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Complete/finalize a booking (for station operators)
        /// </summary>
        [HttpPost("{id}/complete")]
        public async Task<ActionResult<Booking>> CompleteBooking(string id, CompleteBookingRequest request)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound($"Booking with ID {id} not found");
                }

                // Validate QR code if provided
                if (!string.IsNullOrEmpty(request.QRData))
                {
                    var isValid = await _qrCodeService.IsQRCodeValidAsync(request.QRData, id);
                    if (!isValid)
                    {
                        return BadRequest("Invalid QR code for this booking");
                    }
                }

                booking.Status = "Completed";
                booking.UpdatedAt = DateTime.UtcNow;

                var updatedBooking = await _bookingService.UpdateBookingAsync(id, booking);
                return Ok(updatedBooking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class QRValidationRequest
    {
        public string QRData { get; set; } = string.Empty;
    }

    public class QRValidationResponse
    {
        public bool IsValid { get; set; }
        public string BookingId { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public string EVOwnerNIC { get; set; } = string.Empty;
        public string ChargingStationId { get; set; } = string.Empty;
        public int ChargingPointNumber { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class CompleteBookingRequest
    {
        public string QRData { get; set; } = string.Empty;
    }
}