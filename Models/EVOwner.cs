using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingBookingAPI.Models
{
    /// <summary>
    /// Represents an Electric Vehicle Owner
    /// </summary>
    public class EVOwner
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)] // Add this line
        [BsonElement("nic")]
        public string NIC { get; set; } = string.Empty;

        [BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("vehicleModel")]
        public string VehicleModel { get; set; } = string.Empty;

        [BsonElement("vehiclePlateNumber")]
        public string VehiclePlateNumber { get; set; } = string.Empty;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}