using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingBookingAPI.Models
{
    /// <summary>
    /// Represents a system user (Backoffice or Station Operator)
    /// </summary>
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("username")]
        public string Username { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = string.Empty; // "Backoffice" or "StationOperator"

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}