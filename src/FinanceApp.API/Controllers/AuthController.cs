using FinanceApp.Shared.Data;
using FinanceApp.Shared.DTOs;
using FinanceApp.Shared.Helpers;
using FinanceApp.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinanceApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.nm_email == request.Email);

            if (user == null || !PasswordHelper.Verify(request.Senha, user.hs_senha))
            {
                return Unauthorized("Email ou senha inválidos");
            }

            var token = GenerateToken(user, request.RememberMe);

            return Ok(new LoginResponse
            {
                Token = token,
                NomeUsuario = user.nm_nomeUsuario,
                Email = user.nm_email,
                IsAdmin = user.fl_admin
            });
        }

        /* 
         * Registration is now restricted to Administrators via UsersController.
         * Public registration is disabled.
         */

        private string GenerateToken(User user, bool rememberMe = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id_usuario.ToString()),
                new Claim(ClaimTypes.Name, user.nm_nomeUsuario),
                new Claim(ClaimTypes.Email, user.nm_email)
            };

            if (user.fl_admin)
            {
                claims.Add(new Claim("isAdmin", "true"));
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(rememberMe ? 30 : 7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
