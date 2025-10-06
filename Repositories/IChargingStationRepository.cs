using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.Repositories
{
    /// <summary>
    /// Interface for Charging Station data operations
    /// </summary>
    public interface IChargingStationRepository
    {
        Task<List<ChargingStation>> GetAllAsync();
        Task<ChargingStation> GetByIdAsync(string id);
        Task<List<ChargingStation>> GetActiveStationsAsync();
        Task<List<ChargingStation>> GetStationsByTypeAsync(string stationType);
        Task<ChargingStation> CreateAsync(ChargingStation station);
        Task UpdateAsync(string id, ChargingStation station);
        Task<bool> DeactivateAsync(string id);
        Task<bool> DeleteAsync(string id);
        Task<List<ChargingStation>> GetNearbyStationsAsync(double latitude, double longitude, double radiusKm);
    }
}