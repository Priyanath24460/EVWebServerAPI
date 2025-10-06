using Microsoft.AspNetCore.Mvc;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Services;
using EVChargingBookingAPI.Repositories;

namespace EVChargingBookingAPI.Controllers
{
    /// <summary>
    /// Controller for managing EV Owners
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EVOwnersController : ControllerBase
    {
        private readonly IEVOwnerRepository _evOwnerRepository;

        public EVOwnersController(IEVOwnerRepository evOwnerRepository)
        {
            _evOwnerRepository = evOwnerRepository;
        }

        /// <summary>
        /// Get all EV owners
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EVOwner>>> GetAll()
        {
            try
            {
                var evOwners = await _evOwnerRepository.GetAllAsync();
                return Ok(evOwners);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get EV owner by NIC
        /// </summary>
        [HttpGet("{nic}")]
        public async Task<ActionResult<EVOwner>> GetByNIC(string nic)
        {
            try
            {
                var evOwner = await _evOwnerRepository.GetByNICAsync(nic);
                if (evOwner == null)
                {
                    return NotFound($"EV Owner with NIC {nic} not found");
                }
                return Ok(evOwner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new EV owner
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<EVOwner>> Create(EVOwner evOwner)
        {
            try
            {
                // Check if EV Owner already exists
                if (await _evOwnerRepository.ExistsAsync(evOwner.NIC))
                {
                    return Conflict($"EV Owner with NIC {evOwner.NIC} already exists");
                }

                await _evOwnerRepository.CreateAsync(evOwner);
                return CreatedAtAction(nameof(GetByNIC), new { nic = evOwner.NIC }, evOwner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing EV owner
        /// </summary>
        [HttpPut("{nic}")]
        public async Task<ActionResult> Update(string nic, EVOwner evOwner)
        {
            try
            {
                if (nic != evOwner.NIC)
                {
                    return BadRequest("NIC mismatch");
                }

                var existingOwner = await _evOwnerRepository.GetByNICAsync(nic);
                if (existingOwner == null)
                {
                    return NotFound($"EV Owner with NIC {nic} not found");
                }

                await _evOwnerRepository.UpdateAsync(nic, evOwner);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete an EV owner
        /// </summary>
        [HttpDelete("{nic}")]
        public async Task<ActionResult> Delete(string nic)
        {
            try
            {
                var existingOwner = await _evOwnerRepository.GetByNICAsync(nic);
                if (existingOwner == null)
                {
                    return NotFound($"EV Owner with NIC {nic} not found");
                }

                await _evOwnerRepository.DeleteAsync(nic);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Activate/Deactivate an EV owner account
        /// </summary>
        [HttpPatch("{nic}/status")]
        public async Task<ActionResult> UpdateStatus(string nic, [FromBody] bool isActive)
        {
            try
            {
                var existingOwner = await _evOwnerRepository.GetByNICAsync(nic);
                if (existingOwner == null)
                {
                    return NotFound($"EV Owner with NIC {nic} not found");
                }

                existingOwner.IsActive = isActive;
                existingOwner.UpdatedAt = DateTime.UtcNow;

                await _evOwnerRepository.UpdateAsync(nic, existingOwner);
                return Ok($"EV Owner account {(isActive ? "activated" : "deactivated")} successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}