using Microsoft.AspNetCore.Mvc;

namespace EVChargingBookingAPI.Controllers
{
    /// <summary>
    /// Controller for testing CORS and connectivity
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Simple test endpoint to verify CORS and API connectivity
        /// </summary>
        [HttpGet("cors")]
        public ActionResult<object> TestCors()
        {
            return Ok(new
            {
                message = "CORS is working!",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                origin = Request.Headers.ContainsKey("Origin") ? Request.Headers["Origin"].ToString() : "No Origin",
                userAgent = Request.Headers.ContainsKey("User-Agent") ? Request.Headers["User-Agent"].ToString() : "No User-Agent"
            });
        }

        /// <summary>
        /// Test POST endpoint for preflight requests
        /// </summary>
        [HttpPost("cors")]
        public ActionResult<object> TestCorsPost([FromBody] object data)
        {
            return Ok(new
            {
                message = "CORS POST is working!",
                receivedData = data,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public ActionResult<object> HealthCheck()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }

        /// <summary>
        /// Check server time and timezone information
        /// </summary>
        [HttpGet("time")]
        public ActionResult<object> GetServerTime()
        {
            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;
            var timeZone = TimeZoneInfo.Local;

            return Ok(new
            {
                serverLocalTime = now,
                serverUtcTime = utcNow,
                timeZoneName = timeZone.DisplayName,
                timeZoneId = timeZone.Id,
                offsetFromUtc = timeZone.GetUtcOffset(now),
                currentHour = now.Hour,
                utcHour = utcNow.Hour,
                isDaylightSavingTime = timeZone.IsDaylightSavingTime(now)
            });
        }
    }
}