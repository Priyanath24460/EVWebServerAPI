using System.ComponentModel.DataAnnotations;

namespace EVChargingBookingAPI.DTOs
{
    public class UpdateStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}