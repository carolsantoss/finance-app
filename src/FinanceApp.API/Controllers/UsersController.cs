using FinanceApp.Shared.Data;
using FinanceApp.Shared.DTOs;
using FinanceApp.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMe()
        {
            var userId = GetUserId();
            var user = await _context.users.FindAsync(userId);

            if (user == null) return NotFound();

            return Ok(new UserProfileDto
            {
                Id = user.id_usuario,
                NomeUsuario = user.nm_nomeUsuario,
                Email = user.nm_email
            });
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetUserId();
            var user = await _context.users.FindAsync(userId);

            if (user == null) return NotFound();

            if (await _context.users.AnyAsync(u => u.nm_email == request.Email && u.id_usuario != userId))
            {
                return BadRequest("Este email já está em uso.");
            }

            user.nm_nomeUsuario = request.NomeUsuario;
            user.nm_email = request.Email;

            await _context.SaveChangesAsync();

            return Ok(new UserProfileDto
            {
                Id = user.id_usuario,
                NomeUsuario = user.nm_nomeUsuario,
                Email = user.nm_email
            });
        }

        [HttpPut("me/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserId();
            var user = await _context.users.FindAsync(userId);

            if (user == null) return NotFound();

            if (!PasswordHelper.Verify(request.CurrentPassword, user.hs_senha))
            {
                return BadRequest("Senha atual incorreta.");
            }

            user.hs_senha = PasswordHelper.Hash(request.NewPassword);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
