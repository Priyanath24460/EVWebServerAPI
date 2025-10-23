namespace EVChargingBookingAPI.DTOs
{
    /// <summary>
    /// Represents a time slot for a charging point
    /// </summary>
    public class TimeSlotDTO
    {
        public int Hour { get; set; } // 0-23
        public string TimeRange { get; set; } = string.Empty; // e.g., "09:00 - 10:00"
        public bool IsAvailable { get; set; }
        public string? BookedBy { get; set; } // EV Owner NIC if booked
        public string? BookingId { get; set; } // Booking ID if booked
    }

    /// <summary>
    /// Mobile-compatible time slot representation
    /// </summary>
    public class MobileTimeSlotDTO
    {
        public int hour { get; set; } // 0-23 (lowercase for mobile compatibility)
        public string displayTime { get; set; } = string.Empty; // e.g., "09:00"
        public bool isAvailable { get; set; } // lowercase for mobile compatibility
    }

    /// <summary>
    /// Represents available time slots for a charging point
    /// </summary>
    public class ChargingPointSlotsDTO
    {
        public int ChargingPointNumber { get; set; }
        public List<TimeSlotDTO> TimeSlots { get; set; } = new List<TimeSlotDTO>();
    }

    /// <summary>
    /// Mobile-compatible charging point slots
    /// </summary>
    public class MobileChargingPointSlotsDTO
    {
        public int chargingPointNumber { get; set; } // lowercase for mobile compatibility
        public List<MobileTimeSlotDTO> timeSlots { get; set; } = new List<MobileTimeSlotDTO>();
    }

    /// <summary>
    /// Represents available time slots for a charging station
    /// </summary>
    public class StationAvailabilityDTO
    {
        public string StationId { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<ChargingPointSlotsDTO> ChargingPoints { get; set; } = new List<ChargingPointSlotsDTO>();
    }

    /// <summary>
    /// Mobile-compatible station availability response
    /// </summary>
    public class MobileStationAvailabilityDTO
    {
        public string stationId { get; set; } = string.Empty; // lowercase for mobile compatibility
        public string date { get; set; } = string.Empty; // string format for mobile compatibility
        public List<MobileChargingPointSlotsDTO> chargingPoints { get; set; } = new List<MobileChargingPointSlotsDTO>();
    }

    /// <summary>
    /// Request DTO for creating a time slot booking
    /// </summary>
    public class CreateTimeSlotBookingDTO
    {
        public string ChargingStationId { get; set; } = string.Empty;
        public string EVOwnerNIC { get; set; } = string.Empty;
        public int ChargingPointNumber { get; set; } // 1, 2, or 3
        public DateTime BookingDate { get; set; } // Date only
        public int TimeSlot { get; set; } // 0-23 (hour)
    }
}