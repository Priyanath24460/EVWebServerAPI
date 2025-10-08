using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingBookingAPI.Models
{
    /// <summary>
    /// Represents a charging station with available slots
    /// </summary>
    public class ChargingStation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("location")]
        public Location Location { get; set; } = new Location();

        [BsonElement("stationType")]
        public string StationType { get; set; } = string.Empty; // "AC" or "DC"

        [BsonElement("totalSlots")]
        public int TotalSlots { get; set; }

        [BsonElement("availableSlots")]
        public List<TimeSlot> AvailableSlots { get; set; } = new List<TimeSlot>();

        [BsonElement("assignedOperatorId")]
        public string? AssignedOperatorId { get; set; } // ID of the Station Operator assigned to manage this station

        [BsonElement("assignedOperatorUsername")]
        public string? AssignedOperatorUsername { get; set; } // Username of the assigned operator for easy reference

        [BsonElement("assignedOperatorPassword")]
        public string? AssignedOperatorPassword { get; set; } // Password of the assigned operator for display purposes

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("createdByUserId")]
        public string? CreatedByUserId { get; set; } // ID of the Backoffice user who created this station
    }

    public class Location
    {
        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("city")]
        public string City { get; set; } = string.Empty;

        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }
    }

    public class TimeSlot
    {
        [BsonElement("slotId")]
        public string SlotId { get; set; } = string.Empty;

        [BsonElement("startTime")]
        public DateTime StartTime { get; set; }

        [BsonElement("endTime")]
        public DateTime EndTime { get; set; }

        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; } = true;
    }
}