namespace EVChargingBookingAPI.DTOs
{
    public class CreateBookingRequest
    {
        public string EVOwnerNIC { get; set; } = string.Empty;
        public string ChargingStationId { get; set; } = string.Empty;
        public string SlotId { get; set; } = string.Empty;
        public DateTime ReservationDateTime { get; set; }
        public int DurationMinutes { get; set; } = 60;
    }

    public class BookingResponse
    {
        public string BookingReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ReservationDateTime { get; set; }
        public string QRCodeData { get; set; } = string.Empty;
    }
}