using Microsoft.AspNetCore.Mvc;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Services;

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

        public ChargingStationsController(IChargingStationService chargingStationService)
        {
            _chargingStationService = chargingStationService;
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
        /// Create a new charging station
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ChargingStation>> Create(ChargingStation station)
        {
            try
            {
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
        /// Update an existing charging station
        /// </summary>
        [HttpPut("{id}")]
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
        /// Deactivate a charging station
        /// </summary>
        [HttpPatch("{id}/deactivate")]
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
        /// Delete a charging station
        /// </summary>
        [HttpDelete("{id}")]
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
    }
}