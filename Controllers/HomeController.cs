using Microsoft.AspNetCore.Mvc;

namespace EVChargingBookingAPI.Controllers
{
    /// <summary>
    /// Default home controller to provide a simple health/default page at the root path.
    /// </summary>
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("EVChargingBookingAPI is running!");
        }
    }
}
