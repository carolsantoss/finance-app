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

            var token = GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                NomeUsuario = user.nm_nomeUsuario
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            if (await _context.users.AnyAsync(u => u.nm_email == request.Email))
            {
                return BadRequest("Email já cadastrado");
            }

            var user = new User
            {
                nm_nomeUsuario = request.NomeUsuario,
                nm_email = request.Email,
                hs_senha = PasswordHelper.Hash(request.Senha)
            };

            _context.users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                NomeUsuario = user.nm_nomeUsuario
            });
        }

        private string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.id_usuario.ToString()),
                    new Claim(ClaimTypes.Name, user.nm_nomeUsuario),
                    new Claim(ClaimTypes.Email, user.nm_email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
