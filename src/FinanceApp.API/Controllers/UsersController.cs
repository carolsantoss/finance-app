using FinanceApp.Shared.Data;
using FinanceApp.Shared.DTOs;
using FinanceApp.Shared.Helpers;
using FinanceApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.users.ToListAsync();
            return Ok(users.Select(u => new UserDTO 
            {
                Id = u.id_usuario,
                NomeUsuario = u.nm_nomeUsuario,
                Email = u.nm_email,
                IsAdmin = u.fl_admin,
                IsTwoFactorEnabled = u.fl_2faHabilitado,
                JobTitle = u.nm_funcao,
                Phone = u.nr_telefone,
                Bio = u.ds_sobre
            }));
        }

        // GET: api/users/me (Accessible by any authenticated user)
        [HttpGet("me")]
        [AllowAnonymous] // Handles complex logic inside
        public async Task<ActionResult<UserDTO>> GetMe()
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();

            var id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.users.FindAsync(id);

            if (user == null) return NotFound();

            return Ok(new UserDTO
            {
                Id = user.id_usuario,
                NomeUsuario = user.nm_nomeUsuario,
                Email = user.nm_email,
                IsAdmin = user.fl_admin,
                IsTwoFactorEnabled = user.fl_2faHabilitado,
                JobTitle = user.nm_funcao,
                Phone = user.nr_telefone,
                Bio = user.ds_sobre
            });
        }

        // PUT: api/users/me/password
        [HttpPut("me/password")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();

            var id = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.users.FindAsync(id);

            if (user == null) return NotFound();

            if (!PasswordHelper.Verify(request.CurrentPassword, user.hs_senha))
            {
                return BadRequest("Senha atual incorreta.");
            }

            user.hs_senha = PasswordHelper.Hash(request.NewPassword);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser(CreateUserDTO request)
        {
            if (await _context.users.AnyAsync(u => u.nm_email == request.Email))
            {
                return BadRequest("Email já cadastrado");
            }

            var user = new User
            {
                nm_nomeUsuario = request.NomeUsuario,
                nm_email = request.Email,
                hs_senha = PasswordHelper.Hash(request.Senha),
                fl_admin = request.IsAdmin
            };

            _context.users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsers), new { id = user.id_usuario }, new UserDTO
            {
                Id = user.id_usuario,
                NomeUsuario = user.nm_nomeUsuario,
                Email = user.nm_email,
                IsAdmin = user.fl_admin,
                IsTwoFactorEnabled = user.fl_2faHabilitado,
                JobTitle = user.nm_funcao,
                Phone = user.nr_telefone,
                Bio = user.ds_sobre
            });
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO request)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null) return NotFound();

            user.nm_nomeUsuario = request.NomeUsuario;
            user.nm_email = request.Email;
            user.nm_email = request.Email;
            user.fl_admin = request.IsAdmin;
            user.nm_funcao = request.JobTitle;
            user.nr_telefone = request.Phone;
            user.ds_sobre = request.Bio;

            if (!string.IsNullOrEmpty(request.Senha))
            {
                user.hs_senha = PasswordHelper.Hash(request.Senha);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null) return NotFound();

            // Prevent self-deletion
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (id == currentUserId)
            {
                return BadRequest("Você não pode deletar sua própria conta.");
            }

            _context.users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.users.Any(e => e.id_usuario == id);
        }
    }

    // DTOs (Data Transfer Objects) defined here for simplicity, 
    // ideally they should be in Shared/DTOs
    public class UserDTO
    {
        public int Id { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public string? JobTitle { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
    }

    public class CreateUserDTO
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }

    public class UpdateUserDTO
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Senha { get; set; } // Optional
        public bool IsAdmin { get; set; }
        public string? JobTitle { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
    }
}
