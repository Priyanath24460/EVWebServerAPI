using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Repositories;

namespace EVChargingBookingAPI.Services
{
    /// <summary>
    /// Service implementation for Charging Station business logic
    /// </summary>
    public class ChargingStationService : IChargingStationService
    {
        private readonly IChargingStationRepository _chargingStationRepository;

        public ChargingStationService(IChargingStationRepository chargingStationRepository)
        {
            _chargingStationRepository = chargingStationRepository;
        }

        public async Task<List<ChargingStation>> GetAllStationsAsync()
        {
            return await _chargingStationRepository.GetAllAsync();
        }

        public async Task<ChargingStation> GetStationByIdAsync(string id)
        {
            return await _chargingStationRepository.GetByIdAsync(id);
        }

        public async Task<List<ChargingStation>> GetActiveStationsAsync()
        {
            return await _chargingStationRepository.GetActiveStationsAsync();
        }

        public async Task<List<ChargingStation>> GetStationsByTypeAsync(string stationType)
        {
            if (stationType != "AC" && stationType != "DC")
            {
                throw new ArgumentException("Station type must be 'AC' or 'DC'");
            }

            return await _chargingStationRepository.GetStationsByTypeAsync(stationType);
        }

        public async Task<ChargingStation> CreateStationAsync(ChargingStation station)
        {
            // Validate station type
            if (station.StationType != "AC" && station.StationType != "DC")
            {
                throw new ArgumentException("Station type must be 'AC' or 'DC'");
            }

            // Validate location
            if (station.Location == null)
            {
                throw new ArgumentException("Location information is required");
            }

            // Generate initial time slots if not provided
            if (station.AvailableSlots == null || !station.AvailableSlots.Any())
            {
                station.AvailableSlots = GenerateDefaultTimeSlots();
            }

            return await _chargingStationRepository.CreateAsync(station);
        }

        public async Task<ChargingStation> UpdateStationAsync(string id, ChargingStation station)
        {
            var existingStation = await _chargingStationRepository.GetByIdAsync(id);
            if (existingStation == null)
            {
                throw new ArgumentException("Charging station not found");
            }

            // Cannot change station type if there are active bookings
            if (existingStation.StationType != station.StationType)
            {
                throw new InvalidOperationException("Cannot change station type when station is in use");
            }

            await _chargingStationRepository.UpdateAsync(id, station);
            return station;
        }

        public async Task<bool> DeactivateStationAsync(string id)
        {
            return await _chargingStationRepository.DeactivateAsync(id);
        }

        public async Task<bool> DeleteStationAsync(string id)
        {
            return await _chargingStationRepository.DeleteAsync(id);
        }

        public async Task<List<ChargingStation>> GetNearbyStationsAsync(double latitude, double longitude, double radiusKm = 10)
        {
            if (radiusKm <= 0 || radiusKm > 100)
            {
                throw new ArgumentException("Radius must be between 1 and 100 km");
            }

            return await _chargingStationRepository.GetNearbyStationsAsync(latitude, longitude, radiusKm);
        }

        public async Task<bool> AddTimeSlotAsync(string stationId, TimeSlot timeSlot)
        {
            var station = await _chargingStationRepository.GetByIdAsync(stationId);
            if (station == null)
            {
                throw new ArgumentException("Charging station not found");
            }

            // Generate unique slot ID if not provided
            if (string.IsNullOrEmpty(timeSlot.SlotId))
            {
                timeSlot.SlotId = $"SLOT_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            }

            // Check if slot ID already exists
            if (station.AvailableSlots.Any(s => s.SlotId == timeSlot.SlotId))
            {
                throw new InvalidOperationException("Slot ID already exists");
            }

            station.AvailableSlots.Add(timeSlot);
            await _chargingStationRepository.UpdateAsync(stationId, station);
            return true;
        }

        public async Task<bool> RemoveTimeSlotAsync(string stationId, string slotId)
        {
            var station = await _chargingStationRepository.GetByIdAsync(stationId);
            if (station == null)
            {
                throw new ArgumentException("Charging station not found");
            }

            var slotToRemove = station.AvailableSlots.FirstOrDefault(s => s.SlotId == slotId);
            if (slotToRemove == null)
            {
                throw new ArgumentException("Time slot not found");
            }

            station.AvailableSlots.Remove(slotToRemove);
            await _chargingStationRepository.UpdateAsync(stationId, station);
            return true;
        }

        private List<TimeSlot> GenerateDefaultTimeSlots()
        {
            var slots = new List<TimeSlot>();
            var startTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 8, 0, 0);
            
            for (int i = 0; i < 10; i++) // Generate 10 slots per day
            {
                slots.Add(new TimeSlot
                {
                    SlotId = $"SLOT_{i + 1}",
                    StartTime = startTime.AddHours(i),
                    EndTime = startTime.AddHours(i + 1),
                    IsAvailable = true
                });
            }

            return slots;
        }

        public async Task<List<ChargingStation>> GetAllChargingStationsAsync()
        {
            return await GetAllStationsAsync();
        }

        public async Task<ChargingStation> CreateChargingStationAsync(ChargingStation station)
        {
            return await CreateStationAsync(station);
        }
    }
}