using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.DTOs
{
    /// <summary>
    /// DTO for creating a new charging station with operator assignment
    /// </summary>
    public class CreateStationDTO
    {
        public string Name { get; set; } = string.Empty;
        public Location Location { get; set; } = new Location();
        public string StationType { get; set; } = string.Empty; // "AC" or "DC"
        public int TotalSlots { get; set; }
        
        // Operator assignment information
        public string OperatorUsername { get; set; } = string.Empty;
        public string OperatorPassword { get; set; } = string.Empty;
        public string OperatorEmail { get; set; } = string.Empty;
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