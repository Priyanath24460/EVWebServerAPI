
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using EVChargingBookingAPI.Models;

namespace EVChargingBookingAPI.Data
{
    /// <summary>
    /// MongoDB context class for database connection
    /// </summary>
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDBContext(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<EVOwner> EVOwners => _database.GetCollection<EVOwner>("EVOwners");
        public IMongoCollection<ChargingStation> ChargingStations => _database.GetCollection<ChargingStation>("ChargingStations");
        public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");
    }

    public class MongoDBSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}