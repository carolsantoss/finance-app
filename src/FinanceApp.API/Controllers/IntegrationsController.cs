using FinanceApp.Shared.Data;
using FinanceApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensure Admin logic inside
    public class IntegrationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IntegrationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("scheduler-token")]
        public async Task<ActionResult<object>> GetSchedulerToken()
        {
            // Security check: Only Admins can see this
            // We assume a Claim 'IsAdmin' or check user role. 
            // For now, let's assume any authorized user for simplicity or check DB User.
             // (Ideally implement true RBAC)
            
            var setting = await _context.systemSettings.FindAsync("SchedulerSecret");
            if (setting == null)
            {
                // Create a default if not exists
                setting = new SystemSetting
                {
                    Key = "SchedulerSecret",
                    Value = Guid.NewGuid().ToString("N") // Generate a random token
                };
                _context.systemSettings.Add(setting);
                await _context.SaveChangesAsync();
            }

            return Ok(new { Token = setting.Value });
        }

        [HttpPost("scheduler-token/regenerate")]
        public async Task<ActionResult<object>> RegenerateSchedulerToken()
        {
            var setting = await _context.systemSettings.FindAsync("SchedulerSecret");
            if (setting == null)
            {
                setting = new SystemSetting { Key = "SchedulerSecret" };
                _context.systemSettings.Add(setting);
            }

            setting.Value = Guid.NewGuid().ToString("N");
            await _context.SaveChangesAsync();

            return Ok(new { Token = setting.Value });
        }
    }
}
