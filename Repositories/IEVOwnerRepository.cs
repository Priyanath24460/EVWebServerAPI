using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.Repositories
{
    /// <summary>
    /// Interface for EV Owner data operations
    /// </summary>
    public interface IEVOwnerRepository
    {
        Task<List<EVOwner>> GetAllAsync();
        Task<EVOwner> GetByNICAsync(string nic);
        Task CreateAsync(EVOwner evOwner);
        Task UpdateAsync(string nic, EVOwner evOwner);
        Task DeleteAsync(string nic);
        Task<bool> ExistsAsync(string nic);
    }
}