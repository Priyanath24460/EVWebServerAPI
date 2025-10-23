namespace EVChargingBookingAPI.DTOs
{
    public class BookingResponseDTO
    {
        public string BookingId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string QrCodeData { get; set; } = string.Empty;
    }
}