using Microsoft.AspNetCore.Mvc;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Services;

namespace EVChargingBookingAPI.Controllers
{
    /// <summary>
    /// Controller for setting up demo data and testing
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IChargingStationService _chargingStationService;

        public SetupController(IUserService userService, IChargingStationService chargingStationService)
        {
            _userService = userService;
            _chargingStationService = chargingStationService;
        }

        /// <summary>
        /// Initialize the system with default users and charging stations
        /// </summary>
        [HttpPost("initialize")]
        public async Task<ActionResult<string>> InitializeSystem()
        {
            try
            {
                var results = new List<string>();

                // Create default Backoffice user
                try
                {
                    var backofficeUser = new User
                    {
                        Username = "admin",
                        Email = "admin@evsystem.com",
                        Role = "Backoffice",
                        IsActive = true
                    };
                    await _userService.CreateUserAsync(backofficeUser, "admin123");
                    results.Add("✅ Created Backoffice user: admin/admin123");
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
                {
                    results.Add("ℹ️ Backoffice user already exists");
                }

                // Create default Station Operator user
                try
                {
                    var operatorUser = new User
                    {
                        Username = "operator",
                        Email = "operator@evsystem.com",
                        Role = "StationOperator",
                        IsActive = true
                    };
                    await _userService.CreateUserAsync(operatorUser, "operator123");
                    results.Add("✅ Created Station Operator: operator/operator123");
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
                {
                    results.Add("ℹ️ Station Operator user already exists");
                }

                // Create sample charging stations
                var stations = new[]
                {
                    new ChargingStation
                    {
                        Name = "Downtown Mall Station",
                        Location = new Location
                        {
                            Address = "123 Main Street, Colombo 01",
                            City = "Colombo",
                            Latitude = 6.9271,
                            Longitude = 79.8612
                        },
                        StationType = "AC",
                        TotalSlots = 4,
                        AvailableSlots = new List<TimeSlot>
                        {
                            new TimeSlot { SlotId = "A1", StartTime = DateTime.Today.AddHours(8), EndTime = DateTime.Today.AddHours(10), IsAvailable = true },
                            new TimeSlot { SlotId = "A2", StartTime = DateTime.Today.AddHours(10), EndTime = DateTime.Today.AddHours(12), IsAvailable = true },
                            new TimeSlot { SlotId = "A3", StartTime = DateTime.Today.AddHours(14), EndTime = DateTime.Today.AddHours(16), IsAvailable = true },
                            new TimeSlot { SlotId = "A4", StartTime = DateTime.Today.AddHours(16), EndTime = DateTime.Today.AddHours(18), IsAvailable = true }
                        },
                        IsActive = true
                    },
                    new ChargingStation
                    {
                        Name = "Airport Express Station",
                        Location = new Location
                        {
                            Address = "Katunayake Airport Road, Katunayake",
                            City = "Katunayake",
                            Latitude = 7.1679,
                            Longitude = 79.8842
                        },
                        StationType = "DC",
                        TotalSlots = 6,
                        AvailableSlots = new List<TimeSlot>
                        {
                            new TimeSlot { SlotId = "D1", StartTime = DateTime.Today.AddHours(6), EndTime = DateTime.Today.AddHours(8), IsAvailable = true },
                            new TimeSlot { SlotId = "D2", StartTime = DateTime.Today.AddHours(8), EndTime = DateTime.Today.AddHours(10), IsAvailable = true },
                            new TimeSlot { SlotId = "D3", StartTime = DateTime.Today.AddHours(12), EndTime = DateTime.Today.AddHours(14), IsAvailable = true },
                            new TimeSlot { SlotId = "D4", StartTime = DateTime.Today.AddHours(14), EndTime = DateTime.Today.AddHours(16), IsAvailable = true },
                            new TimeSlot { SlotId = "D5", StartTime = DateTime.Today.AddHours(18), EndTime = DateTime.Today.AddHours(20), IsAvailable = true },
                            new TimeSlot { SlotId = "D6", StartTime = DateTime.Today.AddHours(20), EndTime = DateTime.Today.AddHours(22), IsAvailable = true }
                        },
                        IsActive = true
                    }
                };

                foreach (var station in stations)
                {
                    try
                    {
                        await _chargingStationService.CreateChargingStationAsync(station);
                        results.Add($"✅ Created charging station: {station.Name}");
                    }
                    catch (Exception ex)
                    {
                        results.Add($"⚠️ Station {station.Name}: {ex.Message}");
                    }
                }

                return Ok(new
                {
                    message = "System initialization completed",
                    results = results,
                    credentials = new
                    {
                        backoffice = new { username = "admin", password = "admin123", role = "Backoffice" },
                        stationOperator = new { username = "operator", password = "operator123", role = "StationOperator" }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get system status and health check
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<object>> GetSystemStatus()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var stations = await _chargingStationService.GetAllChargingStationsAsync();

                return Ok(new
                {
                    timestamp = DateTime.UtcNow,
                    status = "Running",
                    users = new
                    {
                        total = users.Count,
                        backoffice = users.Count(u => u.Role == "Backoffice"),
                        operators = users.Count(u => u.Role == "StationOperator")
                    },
                    stations = new
                    {
                        total = stations.Count,
                        active = stations.Count(s => s.IsActive),
                        ac_stations = stations.Count(s => s.StationType == "AC"),
                        dc_stations = stations.Count(s => s.StationType == "DC")
                    },
                    endpoints = new
                    {
                        web_app = "Use admin/admin123 or operator/operator123 to login",
                        mobile_auth = "/api/auth/mobile-login",
                        mobile_register = "/api/auth/register-evowner"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Status check failed: {ex.Message}");
            }
        }
    }
}