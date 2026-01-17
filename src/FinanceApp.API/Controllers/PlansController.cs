using FinanceApp.Shared.Data;
using FinanceApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PlansController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlansController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/plans
        // Allow users to see plans? Maybe for upgrading? 
        // For now Admin only based on class Attribute, but specific method override possible.
        [HttpGet]
        [AllowAnonymous] // Anyone can see plans
        public async Task<ActionResult<IEnumerable<PlanDTO>>> GetPlans()
        {
            var plans = await _context.plans
                .Include(p => p.PlanFeatures)
                .ThenInclude(pf => pf.Feature)
                .ToListAsync();

            return Ok(plans.Select(p => new PlanDTO
            {
                Id = p.id_plan,
                Name = p.nm_name,
                Description = p.ds_description,
                Price = p.nr_price,
                IsDefault = p.fl_isDefault,
                Features = p.PlanFeatures.Select(pf => new FeatureDTO
                {
                    Id = pf.Feature.id_feature,
                    Key = pf.Feature.nm_key,
                    Label = pf.Feature.nm_label,
                    Description = pf.Feature.ds_description
                }).ToList()
            }));
        }

        // GET: api/plans/features
        [HttpGet("features")]
        public async Task<ActionResult<IEnumerable<FeatureDTO>>> GetAllFeatures()
        {
            var features = await _context.features.ToListAsync();
            return Ok(features.Select(f => new FeatureDTO
            {
                Id = f.id_feature,
                Key = f.nm_key,
                Label = f.nm_label,
                Description = f.ds_description
            }));
        }

        // POST: api/plans
        [HttpPost]
        public async Task<ActionResult<Plan>> CreatePlan(CreatePlanDTO request)
        {
            var plan = new Plan
            {
                nm_name = request.Name,
                ds_description = request.Description,
                nr_price = request.Price,
                fl_isDefault = request.IsDefault
            };

            if (request.IsDefault)
            {
                // Unset other defaults
                var defaults = await _context.plans.Where(p => p.fl_isDefault).ToListAsync();
                foreach (var d in defaults) d.fl_isDefault = false;
            }

            _context.plans.Add(plan);
            await _context.SaveChangesAsync();

            // Add Features
            if (request.FeatureIds != null && request.FeatureIds.Any())
            {
                foreach (var fid in request.FeatureIds)
                {
                    _context.planFeatures.Add(new PlanFeature { id_plan = plan.id_plan, id_feature = fid });
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetPlans), new { id = plan.id_plan }, plan);
        }

        // PUT: api/plans/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlan(int id, CreatePlanDTO request)
        {
            var plan = await _context.plans
                .Include(p => p.PlanFeatures)
                .FirstOrDefaultAsync(p => p.id_plan == id);

            if (plan == null) return NotFound();

            plan.nm_name = request.Name;
            plan.ds_description = request.Description;
            plan.nr_price = request.Price;

            if (request.IsDefault && !plan.fl_isDefault)
            {
                 var defaults = await _context.plans.Where(p => p.fl_isDefault).ToListAsync();
                 foreach (var d in defaults) d.fl_isDefault = false;
                 plan.fl_isDefault = true;
            }
            else if (!request.IsDefault && plan.fl_isDefault)
            {
                // Cannot unset default if it's the only one, strictly speaking logic depends on business rule.
                // Letting it be false is risky if no default exists.
                plan.fl_isDefault = false;
            }

            // Update Features
            // Remove existing
            _context.planFeatures.RemoveRange(plan.PlanFeatures);
            
            // Add new
            if (request.FeatureIds != null)
            {
                foreach (var fid in request.FeatureIds)
                {
                    _context.planFeatures.Add(new PlanFeature { id_plan = plan.id_plan, id_feature = fid });
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        // DELETE: api/plans/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var plan = await _context.plans.FindAsync(id);
            if (plan == null) return NotFound();

            if (plan.fl_isDefault)
            {
                return BadRequest("Cannot delete the default plan.");
            }

            // Check if users have this plan?
            if (await _context.users.AnyAsync(u => u.id_plan == id))
            {
                return BadRequest("Cannot delete plan with assigned users.");
            }

            _context.plans.Remove(plan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class PlanDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsDefault { get; set; }
        public List<FeatureDTO> Features { get; set; } = new();
    }

    public class FeatureDTO
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CreatePlanDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsDefault { get; set; }
        public List<int>? FeatureIds { get; set; }
    }
}
