using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.Services
{
    /// <summary>
    /// Interface for Charging Station business logic
    /// </summary>
    public interface IChargingStationService
    {
        Task<List<ChargingStation>> GetAllStationsAsync();
        Task<ChargingStation> GetStationByIdAsync(string id);
        Task<List<ChargingStation>> GetActiveStationsAsync();
        Task<List<ChargingStation>> GetStationsByTypeAsync(string stationType);
        Task<ChargingStation> CreateStationAsync(ChargingStation station);
        Task<ChargingStation> UpdateStationAsync(string id, ChargingStation station);
        Task<bool> DeactivateStationAsync(string id);
        Task<bool> DeleteStationAsync(string id);
        Task<List<ChargingStation>> GetNearbyStationsAsync(double latitude, double longitude, double radiusKm = 10);
        Task<bool> AddTimeSlotAsync(string stationId, TimeSlot timeSlot);
        Task<bool> RemoveTimeSlotAsync(string stationId, string slotId);
        Task<List<ChargingStation>> GetAllChargingStationsAsync();
        Task<ChargingStation> CreateChargingStationAsync(ChargingStation station);
    }
}