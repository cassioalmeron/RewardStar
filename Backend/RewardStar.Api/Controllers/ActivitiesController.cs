using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RewardStart.Core;
using RewardStart.Core.Models;
using System.Reflection;

namespace RewardStar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitiesController : ControllerBase
    {
        private readonly RewardStartDbContext _context;

        public ActivitiesController(RewardStartDbContext context)
        {
            _context = context;
        }

        private bool HasChanges(Activity newActivity, Activity existingActivity)
        {
            var properties = typeof(Activity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            return properties.Any(prop => 
            {
                var newValue = prop.GetValue(newActivity);
                var existingValue = prop.GetValue(existingActivity);
                
                if (prop.Name == nameof(EntityBase.Id))
                    return false;

                return !Equals(newValue, existingValue);
            });
        }

        // GET: api/Activities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
        {
            return await _context.Activities
                .OrderBy(x => x.Position)
                .ToListAsync();
        }

        // POST: api/Activities
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Activity>>> PostActivity(IEnumerable<Activity> activities)
        {
            if (!activities.Any())
                return BadRequest("No activities provided!");

            // Obtém todas as activities existentes no banco
            var existingActivities = await _context.Activities.ToListAsync();
            
            // Atribui posições baseadas na ordem de entrada
            var activitiesWithPosition = activities.Select((activity, index) =>
            {
                activity.Position = index + 1;
                return activity;
            }).ToList();

            var newActivities = activitiesWithPosition.Where(a => existingActivities.All(ea => ea.Id != a.Id));
            var updatedActivities = activitiesWithPosition.Where(a => existingActivities.Any(ea => ea.Id == a.Id));
            var activitiesToDelete = existingActivities.Where(ea => !activitiesWithPosition.Any(a => a.Id == ea.Id));

            await _context.Activities.AddRangeAsync(newActivities);

            foreach (var activity in updatedActivities)
            {
                var existingActivity = existingActivities.First(ea => ea.Id == activity.Id);
                if (HasChanges(activity, existingActivity))
                {
                    _context.Entry(existingActivity).CurrentValues.SetValues(activity);
                    _context.Entry(existingActivity).State = EntityState.Modified;
                }
            }

            _context.Activities.RemoveRange(activitiesToDelete);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Activities.AnyAsync())
                {
                    return NotFound();
                }
                throw;
            }

            return await _context.Activities.OrderBy(a => a.Position).ToListAsync();
        }
    }
}
