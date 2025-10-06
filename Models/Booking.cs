using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingBookingAPI.Models
{
    /// <summary>
    /// Represents a booking/reservation for a charging station
    /// </summary>
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("bookingReference")]
        public string BookingReference { get; set; } = string.Empty;

        [BsonElement("evOwnerNIC")]
        public string EVOwnerNIC { get; set; } = string.Empty;

        [BsonElement("chargingStationId")]
        public string ChargingStationId { get; set; } = string.Empty;

        [BsonElement("slotId")]
        public string SlotId { get; set; } = string.Empty;

        [BsonElement("reservationDateTime")]
        public DateTime ReservationDateTime { get; set; }

        [BsonElement("bookingDate")]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [BsonElement("durationMinutes")]
        public int DurationMinutes { get; set; } = 60; // Default 1 hour

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Completed, Cancelled

        [BsonElement("qrCodeData")]
        public string QRCodeData { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}