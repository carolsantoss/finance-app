using FinanceApp.API.Services;
using FinanceApp.Shared.Data;
using FinanceApp.Shared.DTOs;
using FinanceApp.Shared.Helpers;
using FinanceApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OtpNet; // Otp.NET
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
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
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

            // Check 2FA
            if (user.fl_2faHabilitado)
            {
                return Ok(new LoginResponse
                {
                    RequiresTwoFactor = true,
                    Email = user.nm_email // Only email required for next step
                });
            }

            // Lazy Generation
            if (string.IsNullOrEmpty(user.cd_referralCode))
            {
                user.cd_referralCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                await _context.SaveChangesAsync();
            }

            var token = GenerateToken(user, request.RememberMe);

            return Ok(new LoginResponse
            {
                Token = token,
                NomeUsuario = user.nm_nomeUsuario,
                Email = user.nm_email,
                IsAdmin = user.fl_admin,
                ReferralCode = user.cd_referralCode,
                ReferralCount = user.nr_indicacoes
            });

        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.users.AnyAsync(u => u.nm_email == request.Email))
            {
                return BadRequest("Email já cadastrado");
            }

            User? referrer = null;
            if (string.IsNullOrEmpty(request.ReferralCode))
            {
                return BadRequest("Código de convite é obrigatório.");
            }

            referrer = await _context.users.FirstOrDefaultAsync(u => u.cd_referralCode == request.ReferralCode);
            if (referrer == null)
            {
                return BadRequest("Código de convite inválido.");
            }
            
            referrer.nr_indicacoes++;

            var user = new User
            {
                nm_nomeUsuario = request.NomeUsuario,
                nm_email = request.Email,
                hs_senha = PasswordHelper.Hash(request.Senha),
                fl_admin = false, // Public registration is never admin
                cd_referralCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(), // Generate unique code
                id_referrer = referrer?.id_usuario
            };

            // Assign Default Plan
            var defaultPlan = await _context.plans.FirstOrDefaultAsync(p => p.fl_isDefault);
            if (defaultPlan != null)
            {
                user.id_plan = defaultPlan.id_plan;
            }

            _context.users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Conta criada com sucesso" });
        }

        [HttpGet("validate-referral/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateReferral(string code)
        {
            var referrer = await _context.users
                .Where(u => u.cd_referralCode == code)
                .Select(u => new { u.nm_nomeUsuario })
                .FirstOrDefaultAsync();

            if (referrer == null)
            {
                return NotFound(new { message = "Código inválido" });
            }

            return Ok(new { valid = true, referrerName = referrer.nm_nomeUsuario });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.nm_email == request.Email);
            if (user == null) 
            {
                // Return Ok to prevent email enumeration
                return Ok(new { message = "Se o email existir, as instruções foram enviadas." });
            }

            var token = Guid.NewGuid().ToString();
            user.cd_tokenRecuperacao = token;
            user.dt_expiracaoToken = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var resetLink = $"{_configuration["FrontendUrl"] ?? "http://localhost:5173"}/reset-password?token={token}";
            var message = $"<p>Você solicitou a redefinição de senha.</p><p>Clique <a href='{resetLink}'>aqui</a> para redefinir.</p>";
            
            await _emailService.SendEmailAsync(user.nm_email, "Redefinição de Senha - Finance App", message);

            return Ok(new { message = "Se o email existir, as instruções foram enviadas." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _context.users
                .FirstOrDefaultAsync(u => u.cd_tokenRecuperacao == request.Token && u.dt_expiracaoToken > DateTime.UtcNow);

            if (user == null)
            {
                return BadRequest("Token inválido ou expirado.");
            }

            user.hs_senha = PasswordHelper.Hash(request.NewPassword);
            user.cd_tokenRecuperacao = null;
            user.dt_expiracaoToken = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Senha redefinida com sucesso." });
        }

        [Authorize]
        [HttpPost("enable-2fa")]
        public async Task<ActionResult<TwoFactorSetupResponse>> Enable2FA()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.users.FindAsync(userId);
            
            if (user == null) return NotFound();

            if (user.fl_2faHabilitado)
            {
                // Already enabled. Maybe return existing? Or allow reset?
                // For security, usually requires current pass to reset. 
                // Let's assume re-enable always generates new secret.
            }

            var key = KeyGeneration.GenerateRandomKey(20);
            var secret = Base32Encoding.ToString(key);
            
            // Temporary store secret or wait for confirmation?
            // Simple approach: Store it but don't enable fl_2faHabilitado yet.
            // But we already have cd_segredo2FA. If we change it, old one stops working.
            // That's fine for "Setting up".
            
            user.cd_segredo2FA = secret;
            await _context.SaveChangesAsync();

            var qrCodeUri = GenerateQrCodeUri(user.nm_email, secret);

            return Ok(new TwoFactorSetupResponse
            {
                Secret = secret,
                QrCodeUri = qrCodeUri
            });
        }

        [Authorize]
        [HttpPost("confirm-2fa")]
        public async Task<IActionResult> Confirm2FA([FromBody] TwoFactorRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _context.users.FindAsync(userId);
            
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(user.cd_segredo2FA))
                return BadRequest("2FA Setup not initiated.");

            var code = request.Code.Replace(" ", "").Replace("-", "");
            var secretBytes = Base32Encoding.ToBytes(user.cd_segredo2FA);
            var totp = new Totp(secretBytes);
            
            // Verify with window of 30 seconds (standard)
            bool valid = totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!valid)
            {
                return BadRequest("Código inválido.");
            }

            user.fl_2faHabilitado = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "2FA habilitado com sucesso." });
        }

        [HttpPost("login-2fa")]
        public async Task<ActionResult<LoginResponse>> Login2FA([FromBody] TwoFactorRequest request)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.nm_email == request.Email);
            
            if (user == null) return Unauthorized("Usuário não encontrado.");
            
            if (!user.fl_2faHabilitado) return BadRequest("2FA não está habilitado para este usuário.");
            
            // Check manual code or bypass if needed (but 2FA is stricter).
            var code = request.Code.Replace(" ", "").Replace("-", "");
            var secretBytes = Base32Encoding.ToBytes(user.cd_segredo2FA);
            var totp = new Totp(secretBytes);

            bool valid = totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
            
            if (!valid)
            {
                return Unauthorized("Código 2FA inválido.");
            }

            // Lazy Generation
            if (string.IsNullOrEmpty(user.cd_referralCode))
            {
                user.cd_referralCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                await _context.SaveChangesAsync();
            }

            var token = GenerateToken(user, true); // Assuming rememberMe true or passed in request?

            return Ok(new LoginResponse
            {
                Token = token,
                NomeUsuario = user.nm_nomeUsuario,
                Email = user.nm_email,
                IsAdmin = user.fl_admin,
                ReferralCode = user.cd_referralCode,
                ReferralCount = user.nr_indicacoes
            });
        }

        private string GenerateQrCodeUri(string email, string secret)
        {
            // Format: otpauth://totp/Issuer:Email?secret=Secret&issuer=Issuer
            var issuer = "FinanceApp";
            return $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";
        }

        /* 
         * Registration is now restricted to Administrators via UsersController.
         * Public registration is disabled.
         */

        private string GenerateToken(User user, bool rememberMe = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Priority: Environment Variable > Configuration > Throw Exception (matching Program.cs)
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"];
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"];
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey)) throw new InvalidOperationException("Jwt:Key is not configured");

            var key = Encoding.ASCII.GetBytes(jwtKey);
            
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
                Issuer = jwtIssuer,
                Audience = jwtAudience
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
