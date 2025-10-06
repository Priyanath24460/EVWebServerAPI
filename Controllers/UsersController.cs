using Microsoft.AspNetCore.Mvc;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Services;

namespace EVChargingBookingAPI.Controllers
{
    /// <summary>
    /// Controller for managing system users (Backoffice and Station Operators)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        [HttpGet("username/{username}")]
        public async Task<ActionResult<User>> GetByUsername(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound($"User with username {username} not found");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<User>> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request.User, request.Password);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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
        /// Update an existing user
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<User>> Update(string id, User user)
        {
            try
            {
                if (id != user.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var updatedUser = await _userService.UpdateUserAsync(id, user);
                return Ok(updatedUser);
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

        /// <summary>
        /// Delete a user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (result)
                {
                    return Ok("User deleted successfully");
                }
                return BadRequest("Failed to delete user");
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

        /// <summary>
        /// Authenticate user
        /// </summary>
        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthResponse>> Authenticate([FromBody] AuthRequest request)
        {
            try
            {
                var isAuthenticated = await _userService.AuthenticateUserAsync(request.Username, request.Password);
                if (isAuthenticated)
                {
                    var user = await _userService.GetUserByUsernameAsync(request.Username);
                    return Ok(new AuthResponse { Success = true, User = user });
                }
                return Unauthorized(new AuthResponse { Success = false, Message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("{id}/change-password")]
        public async Task<ActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                var result = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);
                if (result)
                {
                    return Ok("Password changed successfully");
                }
                return BadRequest("Failed to change password");
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
    }

    // DTOs for user operations
    public class CreateUserRequest
    {
        public User User { get; set; } = new User();
        public string Password { get; set; } = string.Empty;
    }

    public class AuthRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}