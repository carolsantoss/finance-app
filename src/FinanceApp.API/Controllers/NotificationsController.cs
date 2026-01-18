using FinanceApp.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int limit = 20)
        {
            try
            {
                var userId = GetUserId();
                Console.WriteLine($"[NotificationsController] Fetching for UserId: {userId}");
                
                var notifications = await _context.notifications
                    .Where(n => n.id_usuario == userId)
                    .OrderByDescending(n => n.dt_criacao)
                    .Take(limit)
                    .ToListAsync();

                Console.WriteLine($"[NotificationsController] Found {notifications.Count} notifications.");

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                var count = await _context.notifications
                    .CountAsync(n => n.id_usuario == userId && !n.fl_lida);

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = GetUserId();
                var notification = await _context.notifications
                    .FirstOrDefaultAsync(n => n.id_notificacao == id && n.id_usuario == userId);

                if (notification == null)
                    return NotFound();

                notification.fl_lida = true;
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                var unreadNotifications = await _context.notifications
                    .Where(n => n.id_usuario == userId && !n.fl_lida)
                    .ToListAsync();

                if (unreadNotifications.Any())
                {
                    foreach (var n in unreadNotifications)
                    {
                        n.fl_lida = true;
                    }
                    await _context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
