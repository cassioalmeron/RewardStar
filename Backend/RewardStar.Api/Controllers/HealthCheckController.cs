using Microsoft.AspNetCore.Mvc;
using RewardStart.Core;

namespace RewardStar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : ControllerBase
    {
        private readonly RewardStartDbContext _context;

        public HealthCheckController(RewardStartDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Health check endpoint to verify API and database connectivity
        /// </summary>
        /// <returns>Health status with timestamp and database connectivity information</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<HealthCheckResponse>> Get()
        {
            var response = new HealthCheckResponse
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Version = GetApplicationVersion()
            };

            try
            {
                // Check database connectivity
                var canConnect = await _context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    response.Status = "Unhealthy";
                    response.DatabaseConnected = false;
                    response.ErrorMessage = "Unable to connect to database";
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
                }

                response.DatabaseConnected = true;
            }
            catch (Exception ex)
            {
                response.Status = "Unhealthy";
                response.DatabaseConnected = false;
                response.ErrorMessage = $"Database check failed: {ex.Message}";
                return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
            }

            return Ok(response);
        }

        /// <summary>
        /// Simple health check endpoint (used by Docker health checks)
        /// </summary>
        [HttpGet("simple")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Simple()
        {
            return Ok("OK");
        }

        private string GetApplicationVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString() ?? "Unknown";
            return version;
        }
    }

    public class HealthCheckResponse
    {
        public string Status { get; set; } = "Healthy";
        public DateTime Timestamp { get; set; }
        public string Version { get; set; } = string.Empty;
        public bool DatabaseConnected { get; set; } = true;
        public string? ErrorMessage { get; set; }
    }
}
