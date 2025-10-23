using EVChargingBookingAPI.Models;
using System.Text.Json;

namespace EVChargingBookingAPI.Services
{
    /// <summary>
    /// Service for QR Code generation and validation
    /// </summary>
    public interface IQRCodeService
    {
        Task<string> GenerateQRCodeDataAsync(Booking booking);
        Task<QRCodeData> ValidateQRCodeAsync(string qrData);
        Task<bool> IsQRCodeValidAsync(string qrData, string bookingId);
    }

    public class QRCodeService : IQRCodeService
    {
        private readonly IBookingService _bookingService;

        public QRCodeService(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public Task<string> GenerateQRCodeDataAsync(Booking booking)
        {
            if (booking == null || booking.Status != "Approved")
            {
                throw new InvalidOperationException("QR Code can only be generated for approved bookings");
            }

            var qrData = new QRCodeData
            {
                BookingId = booking.Id,
                BookingReference = booking.BookingReference,
                EVOwnerNIC = booking.EVOwnerNIC,
                ChargingStationId = booking.ChargingStationId,
                ChargingPointNumber = booking.ChargingPointNumber,
                StartTime = booking.StartTime,
                DurationMinutes = booking.DurationMinutes,
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = booking.StartTime.AddHours(1), // QR expires 1 hour after reservation time
                Version = "1.0"
            };

            var jsonData = JsonSerializer.Serialize(qrData);
            var encodedData = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData));
            
            return Task.FromResult(encodedData);
        }

        public async Task<QRCodeData> ValidateQRCodeAsync(string qrData)
        {
            try
            {
                var decodedData = Convert.FromBase64String(qrData);
                var jsonData = System.Text.Encoding.UTF8.GetString(decodedData);
                var qrCodeData = JsonSerializer.Deserialize<QRCodeData>(jsonData);

                if (qrCodeData == null)
                {
                    throw new InvalidOperationException("Invalid QR code format");
                }

                // Check if QR code has expired
                if (DateTime.UtcNow > qrCodeData.ExpiresAt)
                {
                    throw new InvalidOperationException("QR code has expired");
                }

                // Verify booking exists and is valid
                var booking = await _bookingService.GetBookingByIdAsync(qrCodeData.BookingId);
                if (booking == null || booking.Status != "Approved")
                {
                    throw new InvalidOperationException("Invalid booking or booking not approved");
                }

                // Verify booking details match QR code
                if (booking.BookingReference != qrCodeData.BookingReference ||
                    booking.EVOwnerNIC != qrCodeData.EVOwnerNIC ||
                    booking.ChargingStationId != qrCodeData.ChargingStationId)
                {
                    throw new InvalidOperationException("QR code data does not match booking details");
                }

                return qrCodeData;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException("Invalid QR code format", ex);
            }
        }

        public async Task<bool> IsQRCodeValidAsync(string qrData, string bookingId)
        {
            try
            {
                var qrCodeData = await ValidateQRCodeAsync(qrData);
                return qrCodeData.BookingId == bookingId;
            }
            catch
            {
                return false;
            }
        }
    }

    public class QRCodeData
    {
        public string BookingId { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public string EVOwnerNIC { get; set; } = string.Empty;
        public string ChargingStationId { get; set; } = string.Empty;
        public int ChargingPointNumber { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Version { get; set; } = "1.0";
    }
}