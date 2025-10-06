using Microsoft.AspNetCore.Mvc;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Services;
using EVChargingBookingAPI.DTOs;
using EVChargingBookingAPI.Middleware;
using System.Linq;

namespace EVChargingBookingAPI.Controllers
{
    /// <summary>
    /// Controller for managing charging stations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ChargingStationsController : ControllerBase
    {
        private readonly IChargingStationService _chargingStationService;
        private readonly IUserService _userService;

        public ChargingStationsController(IChargingStationService chargingStationService, IUserService userService)
        {
            _chargingStationService = chargingStationService;
            _userService = userService;
        }

        /// <summary>
        /// Get all charging stations
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ChargingStation>>> GetAll()
        {
            try
            {
                var stations = await _chargingStationService.GetAllStationsAsync();
                return Ok(stations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get charging station by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ChargingStation>> GetById(string id)
        {
            try
            {
                var station = await _chargingStationService.GetStationByIdAsync(id);
                if (station == null)
                {
                    return NotFound($"Charging station with ID {id} not found");
                }
                return Ok(station);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get active charging stations
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<List<ChargingStation>>> GetActive()
        {
            try
            {
                var stations = await _chargingStationService.GetActiveStationsAsync();
                return Ok(stations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get charging stations by type (AC/DC)
        /// </summary>
        [HttpGet("type/{stationType}")]
        public async Task<ActionResult<List<ChargingStation>>> GetByType(string stationType)
        {
            try
            {
                var stations = await _chargingStationService.GetStationsByTypeAsync(stationType);
                return Ok(stations);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get nearby charging stations
        /// </summary>
        [HttpGet("nearby")]
        public async Task<ActionResult<List<ChargingStation>>> GetNearby([FromQuery] double latitude, [FromQuery] double longitude, [FromQuery] double radiusKm = 10)
        {
            try
            {
                var stations = await _chargingStationService.GetNearbyStationsAsync(latitude, longitude, radiusKm);
                return Ok(stations);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new charging station (Backoffice only)
        /// </summary>
        [HttpPost]
        [RequireRole("Backoffice")]
        public async Task<ActionResult<ChargingStation>> Create(ChargingStation station)
        {
            try
            {
                var userId = HttpContext.Items["UserId"]?.ToString();
                station.CreatedByUserId = userId;
                var createdStation = await _chargingStationService.CreateStationAsync(station);
                return CreatedAtAction(nameof(GetById), new { id = createdStation.Id }, createdStation);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new charging station with auto-generated operator (Backoffice only)
        /// </summary>
        [HttpPost("with-operator")]
        [RequireRole("Backoffice")]
        public async Task<ActionResult<CreateStationResponseDTO>> CreateWithOperator(CreateStationDTO createStationDto)
        {
            try
            {
                var userId = HttpContext.Items["UserId"]?.ToString();
                
                // Generate unique username and password
                var stationCode = createStationDto.Name.Replace(" ", "").ToLower();
                var uniqueId = Guid.NewGuid().ToString().Substring(0, 6);
                var generatedUsername = $"{stationCode}_op_{uniqueId}";
                var generatedPassword = GenerateSecurePassword();
                var generatedEmail = $"{generatedUsername}@evcharging.system";

                // Create the Station Operator with generated credentials
                var newOperator = await _userService.CreateStationOperatorAsync(
                    generatedUsername,
                    generatedPassword,
                    generatedEmail
                );

                // Create the charging station
                var station = new ChargingStation
                {
                    Name = createStationDto.Name,
                    Location = createStationDto.Location,
                    StationType = createStationDto.StationType,
                    TotalSlots = createStationDto.TotalSlots,
                    AssignedOperatorId = newOperator.Id,
                    AssignedOperatorUsername = newOperator.Username,
                    CreatedByUserId = userId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdStation = await _chargingStationService.CreateStationAsync(station);
                
                // Return station with generated credentials
                var response = new CreateStationResponseDTO
                {
                    Station = createdStation,
                    GeneratedUsername = generatedUsername,
                    GeneratedPassword = generatedPassword
                };

                return CreatedAtAction(nameof(GetById), new { id = createdStation.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GenerateSecurePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Update an existing charging station (Backoffice only)
        /// </summary>
        [HttpPut("{id}")]
        [RequireRole("Backoffice")]
        public async Task<ActionResult<ChargingStation>> Update(string id, ChargingStation station)
        {
            try
            {
                if (id != station.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var updatedStation = await _chargingStationService.UpdateStationAsync(id, station);
                return Ok(updatedStation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deactivate a charging station (Backoffice only)
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [RequireRole("Backoffice")]
        public async Task<ActionResult> Deactivate(string id)
        {
            try
            {
                var result = await _chargingStationService.DeactivateStationAsync(id);
                if (result)
                {
                    return Ok("Charging station deactivated successfully");
                }
                return BadRequest("Cannot deactivate station with active bookings");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a charging station (Backoffice only)
        /// </summary>
        [HttpDelete("{id}")]
        [RequireRole("Backoffice")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var result = await _chargingStationService.DeleteStationAsync(id);
                if (result)
                {
                    return Ok("Charging station deleted successfully");
                }
                return BadRequest("Cannot delete station with existing bookings");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Add a time slot to a charging station
        /// </summary>
        [HttpPost("{id}/timeslots")]
        public async Task<ActionResult> AddTimeSlot(string id, [FromBody] TimeSlot timeSlot)
        {
            try
            {
                var result = await _chargingStationService.AddTimeSlotAsync(id, timeSlot);
                if (result)
                {
                    return Ok("Time slot added successfully");
                }
                return BadRequest("Failed to add time slot");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove a time slot from a charging station
        /// </summary>
        [HttpDelete("{id}/timeslots/{slotId}")]
        public async Task<ActionResult> RemoveTimeSlot(string id, string slotId)
        {
            try
            {
                var result = await _chargingStationService.RemoveTimeSlotAsync(id, slotId);
                if (result)
                {
                    return Ok("Time slot removed successfully");
                }
                return BadRequest("Failed to remove time slot");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // ============ ROLE-BASED ENDPOINTS ============

        /// <summary>
        /// Get stations assigned to the current Station Operator
        /// </summary>
        [HttpGet("my-stations")]
        [RequireRole("StationOperator")]
        public async Task<ActionResult<List<ChargingStation>>> GetMyStations()
        {
            try
            {
                var operatorId = HttpContext.Items["UserId"]?.ToString();
                if (string.IsNullOrEmpty(operatorId))
                {
                    return Unauthorized("Operator ID not found");
                }

                var stations = await _chargingStationService.GetStationsByOperatorIdAsync(operatorId);
                return Ok(stations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update station details (Station Operator can only update their assigned stations)
        /// </summary>
        [HttpPut("{id}/operator-update")]
        [RequireRole("StationOperator")]
        public async Task<ActionResult> UpdateByOperator(string id, OperatorStationUpdateDTO updateDto)
        {
            try
            {
                var operatorId = HttpContext.Items["UserId"]?.ToString();
                if (string.IsNullOrEmpty(operatorId))
                {
                    return Unauthorized("Operator ID not found");
                }

                // Check if the station belongs to this operator
                var station = await _chargingStationService.GetStationByIdAsync(id);
                if (station == null)
                {
                    return NotFound("Charging station not found");
                }

                if (station.AssignedOperatorId != operatorId)
                {
                    return Forbid("You can only update stations assigned to you");
                }

                // Update the station
                if (updateDto.AvailableSlots != null)
                {
                    var result = await _chargingStationService.UpdateStationByOperatorAsync(id, operatorId, updateDto.AvailableSlots);
                    if (!result)
                    {
                        return BadRequest("Failed to update station");
                    }
                }

                if (updateDto.IsActive.HasValue)
                {
                    station.IsActive = updateDto.IsActive.Value;
                    await _chargingStationService.UpdateStationAsync(id, station);
                }

                return Ok("Station updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all Station Operators (Backoffice only)
        /// </summary>
        [HttpGet("operators")]
        [RequireRole("Backoffice")]
        public async Task<ActionResult<List<User>>> GetAllOperators()
        {
            try
            {
                var operators = await _userService.GetUsersByRoleAsync("StationOperator");
                
                // Remove password hashes from response
                var safeOperators = operators.Select(op => new User
                {
                    Id = op.Id,
                    Username = op.Username,
                    Email = op.Email,
                    Role = op.Role,
                    IsActive = op.IsActive,
                    CreatedAt = op.CreatedAt
                }).ToList();
                
                return Ok(safeOperators);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Assign an existing operator to a station (Backoffice only)
        /// </summary>
        [HttpPost("{stationId}/assign-operator/{operatorId}")]
        [RequireRole("Backoffice")]
        public async Task<ActionResult> AssignOperatorToStation(string stationId, string operatorId)
        {
            try
            {
                // Validate that the operator exists and is active
                var operatorUser = await _userService.GetUserByIdAsync(operatorId);
                if (operatorUser == null || operatorUser.Role != "StationOperator" || !operatorUser.IsActive)
                {
                    return BadRequest("Invalid or inactive operator");
                }

                var result = await _chargingStationService.AssignOperatorToStationAsync(stationId, operatorId);
                if (!result)
                {
                    return NotFound("Charging station not found");
                }

                return Ok("Operator assigned to station successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deactivate a Station Operator (Backoffice only)
        /// </summary>
        [HttpPatch("operators/{operatorId}/deactivate")]
        [RequireRole("Backoffice")]
        public async Task<ActionResult> DeactivateOperator(string operatorId)
        {
            try
            {
                var result = await _userService.DeactivateUserAsync(operatorId);
                if (!result)
                {
                    return NotFound("Operator not found");
                }

                return Ok("Operator deactivated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}