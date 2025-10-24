using Microsoft.AspNetCore.Mvc;
using EVChargingBookingAPI.Models;
using EVChargingBookingAPI.Services;
using System.Security.Cryptography;
using System.Text;

namespace EVChargingBookingAPI.Controllers
{
    /// <summary>
    /// Controller for authentication operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Login endpoint for web application users
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Username and password are required");
                }

                var user = await _userService.GetUserByUsernameAsync(request.Username);
                if (user == null || !user.IsActive)
                {
                    return Unauthorized("Invalid credentials or user is inactive");
                }

                // Verify password (in production, use proper password hashing)
                var hashedPassword = HashPassword(request.Password);
                if (user.PasswordHash != hashedPassword)
                {
                    return Unauthorized("Invalid credentials");
                }

                var response = new LoginResponse
                {
                    Success = true,
                    Token = GenerateToken(user),
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        IsActive = user.IsActive
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Mobile login endpoint for EV Owners and Station Operators
        /// </summary>
        [HttpPost("mobile-login")]
        public async Task<ActionResult<MobileLoginResponse>> MobileLogin(MobileLoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.NIC))
                {
                    return BadRequest("NIC is required");
                }

                // Check if it's an EV Owner
                var evOwner = await _userService.GetEVOwnerByNICAsync(request.NIC);
                if (evOwner != null && evOwner.IsActive)
                {
                    var response = new MobileLoginResponse
                    {
                        Success = true,
                        UserType = "EVOwner",
                        Token = GenerateToken(evOwner),
                        EVOwner = evOwner
                    };
                    return Ok(response);
                }

                // Check if it's a Station Operator
                if (!string.IsNullOrEmpty(request.Username) && !string.IsNullOrEmpty(request.Password))
                {
                    var user = await _userService.GetUserByUsernameAsync(request.Username);
                    if (user != null && user.Role == "StationOperator" && user.IsActive)
                    {
                        var hashedPassword = HashPassword(request.Password);
                        if (user.PasswordHash == hashedPassword)
                        {
                            var response = new MobileLoginResponse
                            {
                                Success = true,
                                UserType = "StationOperator",
                                Token = GenerateToken(user),
                                StationOperator = user
                            };
                            return Ok(response);
                        }
                    }
                }

                return Unauthorized("Invalid credentials or user not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Register new EV Owner (for mobile app)
        /// </summary>
        [HttpPost("register-evowner")]
        public async Task<ActionResult<EVOwner>> RegisterEVOwner(EVOwner evOwner)
        {
            try
            {
                // Check if EV Owner already exists
                var existing = await _userService.GetEVOwnerByNICAsync(evOwner.NIC);
                if (existing != null)
                {
                    return BadRequest("EV Owner with this NIC already exists");
                }

                evOwner.CreatedAt = DateTime.UtcNow;
                evOwner.UpdatedAt = DateTime.UtcNow;
                evOwner.IsActive = true;

                var createdOwner = await _userService.CreateEVOwnerAsync(evOwner);
                return CreatedAtAction(nameof(RegisterEVOwner), new { nic = createdOwner.NIC }, createdOwner);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create Backoffice user (for registration)
        /// </summary>
        [HttpPost("create-backoffice")]
        public async Task<ActionResult<User>> CreateBackofficeUser(BackofficeCreationRequest request)
        {
            try
            {
                // Check if username already exists
                var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
                if (existingUser != null)
                {
                    return BadRequest("Username already exists. Please choose a different username.");
                }

                var newUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Role = "Backoffice",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userService.CreateUserAsync(newUser, request.Password);
                
                var userResponse = new User
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    Role = createdUser.Role,
                    IsActive = createdUser.IsActive,
                    CreatedAt = createdUser.CreatedAt
                };

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string GenerateToken(object user)
        {
            // In production, use proper JWT token generation
            // For now, return a simple token based on user data
            var userJson = System.Text.Json.JsonSerializer.Serialize(user);
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(userJson));
        }

        /// <summary>
        /// Update Station Operator credentials
        /// </summary>
        [HttpPut("users/{username}/update-credentials")]
        public async Task<ActionResult<string>> UpdateOperatorCredentials(string username, [FromBody] UpdateCredentialsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Username and password are required");
                }

                if (request.Password.Length < 6)
                {
                    return BadRequest("Password must be at least 6 characters long");
                }

                // Get the current user
                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null || user.Role != "StationOperator")
                {
                    return NotFound("Station Operator not found");
                }

                // Check if new username already exists (for different user)
                if (request.Username != username)
                {
                    var existingUser = await _userService.GetUserByUsernameAsync(request.Username);
                    if (existingUser != null)
                    {
                        return BadRequest("Username already exists");
                    }
                }

                // Update the credentials
                user.Username = request.Username;
                user.PasswordHash = HashPassword(request.Password);

                await _userService.UpdateUserAsync(user.Id, user);

                return Ok("Credentials updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new UserInfo();
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class MobileLoginRequest
    {
        public string NIC { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class MobileLoginResponse
    {
        public bool Success { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public EVOwner? EVOwner { get; set; }
        public User? StationOperator { get; set; }
    }

    public class StationOperatorRegistration
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class BackofficeCreationRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateCredentialsRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}