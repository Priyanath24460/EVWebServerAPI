using MongoDB.Driver;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Data;

namespace EVChargingBookingAPI.Repositories
{
    /// <summary>
    /// Repository implementation for Charging Station operations
    /// </summary>
    public class ChargingStationRepository : IChargingStationRepository
    {
        private readonly MongoDBContext _context;

        public ChargingStationRepository(MongoDBContext context)
        {
            _context = context;
        }

        public async Task<List<ChargingStation>> GetAllAsync()
        {
            return await _context.ChargingStations.Find(_ => true).ToListAsync();
        }

        public async Task<ChargingStation> GetByIdAsync(string id)
        {
            return await _context.ChargingStations.Find(cs => cs.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<ChargingStation>> GetActiveStationsAsync()
        {
            return await _context.ChargingStations.Find(cs => cs.IsActive).ToListAsync();
        }

        public async Task<List<ChargingStation>> GetStationsByTypeAsync(string stationType)
        {
            return await _context.ChargingStations
                .Find(cs => cs.StationType == stationType && cs.IsActive)
                .ToListAsync();
        }

        public async Task<ChargingStation> CreateAsync(ChargingStation station)
        {
            await _context.ChargingStations.InsertOneAsync(station);
            return station;
        }

        public async Task UpdateAsync(string id, ChargingStation station)
        {
            await _context.ChargingStations.ReplaceOneAsync(cs => cs.Id == id, station);
        }

        public async Task<bool> DeactivateAsync(string id)
        {
            // Check if station has active bookings
            var activeStatuses = new[] { "Pending", "Approved" };
            var hasActiveBookings = await _context.Bookings
                .Find(b => b.ChargingStationId == id && activeStatuses.Contains(b.Status))
                .AnyAsync();

            if (hasActiveBookings)
            {
                return false; // Cannot deactivate station with active bookings
            }

            var station = await GetByIdAsync(id);
            if (station != null)
            {
                station.IsActive = false;
                await UpdateAsync(id, station);
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            // Check if station has any bookings
            var hasBookings = await _context.Bookings.Find(b => b.ChargingStationId == id).AnyAsync();
            if (hasBookings)
            {
                return false; // Cannot delete station with bookings
            }

            var result = await _context.ChargingStations.DeleteOneAsync(cs => cs.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<List<ChargingStation>> GetNearbyStationsAsync(double latitude, double longitude, double radiusKm)
        {
            var allStations = await GetActiveStationsAsync();
            
            // Simple distance calculation (for production, use geospatial queries)
            var nearbyStations = allStations.Where(station =>
            {
                var distance = CalculateDistance(latitude, longitude, 
                    station.Location.Latitude, station.Location.Longitude);
                return distance <= radiusKm;
            }).ToList();

            return nearbyStations;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Earth's radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        public async Task<List<ChargingStation>> GetByOperatorIdAsync(string operatorId)
        {
            return await _context.ChargingStations
                .Find(cs => cs.AssignedOperatorId == operatorId && cs.IsActive)
                .ToListAsync();
        }
    }
}