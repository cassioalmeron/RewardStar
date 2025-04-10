using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RewardStart.Core;
using RewardStart.Core.Models;

namespace RewardStar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly RewardStartDbContext _context;

        public GameController(RewardStartDbContext context)
        {
            _context = context;
        }

        // GET: api/Game
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
            => await _context.Activities
            .Where(x => x.Active)
            .OrderBy(x => x.Position)
            .ToListAsync();
    }
}
