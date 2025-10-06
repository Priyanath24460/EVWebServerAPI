using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.DTOs
{
    /// <summary>
    /// DTO for creating a new charging station with auto-generated operator
    /// </summary>
    public class CreateStationDTO
    {
        public string Name { get; set; } = string.Empty;
        public Location Location { get; set; } = new Location();
        public string StationType { get; set; } = string.Empty; // "AC" or "DC"
        public int TotalSlots { get; set; }
    }

    /// <summary>
    /// Response DTO for station creation with generated operator credentials
    /// </summary>
    public class CreateStationResponseDTO
    {
        public ChargingStation Station { get; set; } = new ChargingStation();
        public string GeneratedUsername { get; set; } = string.Empty;
        public string GeneratedPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for updating charging station information
    /// </summary>
    public class UpdateStationDTO
    {
        public string? Name { get; set; }
        public Location? Location { get; set; }
        public string? StationType { get; set; }
        public int? TotalSlots { get; set; }
        public bool? IsActive { get; set; }
        
        // Optional: reassign to different operator
        public string? NewOperatorId { get; set; }
    }

    /// <summary>
    /// DTO for Station Operator to update their station details
    /// </summary>
    public class OperatorStationUpdateDTO
    {
        public List<TimeSlot>? AvailableSlots { get; set; }
        public bool? IsActive { get; set; }
    }
}