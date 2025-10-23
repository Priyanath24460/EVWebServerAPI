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

        [BsonElement("chargingPointNumber")]
        public int ChargingPointNumber { get; set; } // 1, 2, or 3

        [BsonElement("bookingDate")]
        public DateTime BookingDate { get; set; } // Date only (YYYY-MM-DD)

        [BsonElement("timeSlot")]
        public int TimeSlot { get; set; } // 0-23 (hour of the day)

        [BsonElement("startTime")]
        public DateTime StartTime { get; set; } // Full start datetime

        [BsonElement("endTime")]
        public DateTime EndTime { get; set; } // Full end datetime

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