using FinanceApp.API.Services;
using FinanceApp.Shared.Data;
using FinanceApp.Shared.DTOs;
using FinanceApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace FinanceApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public InvoicesController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: api/invoices
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<InvoiceDTO>>> GetInvoices()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var invoices = await _context.invoices
                .Where(i => i.id_usuario == userId)
                .ToListAsync();

            var today = DateTime.Today;

            return Ok(invoices.Select(i => new InvoiceDTO
            {
                Id = i.id_invoice,
                Descricao = i.nm_descricao,
                Valor = i.nr_valor,
                DiaVencimento = i.nr_diaVencimento,
                Ativo = i.fl_ativo,
                UltimoPagamento = i.dt_ultimoPagamento,
                PagoEsteMes = i.dt_ultimoPagamento.HasValue && 
                              i.dt_ultimoPagamento.Value.Month == today.Month && 
                              i.dt_ultimoPagamento.Value.Year == today.Year
            }));
        }

        // POST: api/invoices
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Invoice>> CreateInvoice(CreateInvoiceDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (request.DiaVencimento < 1 || request.DiaVencimento > 31)
                return BadRequest("Dia de vencimento inválido (1-31).");

            var invoice = new Invoice
            {
                id_usuario = userId,
                nm_descricao = request.Descricao,
                nr_valor = request.Valor,
                nr_diaVencimento = request.DiaVencimento,
                fl_ativo = true
            };

            _context.invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInvoices), new { id = invoice.id_invoice }, invoice);
        }

        // PUT: api/invoices/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateInvoice(int id, UpdateInvoiceDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var invoice = await _context.invoices.FirstOrDefaultAsync(i => i.id_invoice == id && i.id_usuario == userId);

            if (invoice == null) return NotFound();

            if (request.DiaVencimento < 1 || request.DiaVencimento > 31)
                return BadRequest("Dia de vencimento inválido (1-31).");

            invoice.nm_descricao = request.Descricao;
            invoice.nr_valor = request.Valor;
            invoice.nr_diaVencimento = request.DiaVencimento;
            invoice.fl_ativo = request.Ativo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // PUT: api/invoices/{id}/pay
        [HttpPut("{id}/pay")]
        [Authorize]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var invoice = await _context.invoices.FirstOrDefaultAsync(i => i.id_invoice == id && i.id_usuario == userId);

            if (invoice == null) return NotFound();

            invoice.dt_ultimoPagamento = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Fatura marcada como paga." });
        }
        
         // DELETE: api/invoices/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var invoice = await _context.invoices.FirstOrDefaultAsync(i => i.id_invoice == id && i.id_usuario == userId);

            if (invoice == null) return NotFound();

            _context.invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        // POST: api/invoices/process
        // Scheduler Endpoint
        [HttpPost("process")]
        [AllowAnonymous] 
        public async Task<IActionResult> ProcessInvoices([FromHeader(Name = "X-Scheduler-Secret")] string? secret)
        {
            var log = new IntegrationLog { nm_integration = "Scheduler/Invoices", ds_ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown" };

            try
            {
                // Security Check
                var dbSecret = (await _context.systemSettings.FindAsync("SchedulerSecret"))?.Value;
                var envSecret = Environment.GetEnvironmentVariable("SCHEDULER_SECRET") ?? "DefaultSecret123";
                
                if (secret != dbSecret && secret != envSecret)
                {
                    log.ds_status = "Error";
                    log.ds_message = "Authentication Failed";
                    log.ds_details = "Invalid Secret Key provided.";
                    _context.integrationLogs.Add(log);
                    await _context.SaveChangesAsync();
                    return Unauthorized("Invalid Scheduler Secret");
                }

                var today = DateTime.Today;
                var activeInvoices = await _context.invoices
                    .Include(i => i.Usuario)
                    .Where(i => i.fl_ativo)
                    .ToListAsync();

                // Group by User to send consolidated email
                var userInvoices = activeInvoices
                    .GroupBy(i => i.id_usuario);

                int emailsSent = 0;
                var details = new List<string>();

                foreach (var group in userInvoices)
                {
                    var user = group.First().Usuario;
                    if (user == null || string.IsNullOrEmpty(user.nm_email)) continue;

                    var pendingInvoices = new List<(Invoice Invoice, DateTime DueDate, double DaysUntilDue)>();

                    foreach (var invoice in group)
                    {
                        // Check if already paid this month
                        bool paidThisMonth = invoice.dt_ultimoPagamento.HasValue &&
                                             invoice.dt_ultimoPagamento.Value.Month == today.Month &&
                                             invoice.dt_ultimoPagamento.Value.Year == today.Year;

                        if (paidThisMonth) continue;

                        // Calculate Due Date for THIS Month
                        int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
                        int day = Math.Min(invoice.nr_diaVencimento, daysInMonth);
                        var dueDate = new DateTime(today.Year, today.Month, day);

                        var daysUntilDue = (dueDate - today).TotalDays;

                        // Alert rule: Within 5 days OR Overdue
                        if (daysUntilDue <= 5)
                        {
                            pendingInvoices.Add((invoice, dueDate, daysUntilDue));
                        }
                    }

                    if (pendingInvoices.Any())
                    {
                        // Generate Consolidated Email
                        string subject = pendingInvoices.Any(p => p.DaysUntilDue < 0) 
                            ? "🔴 ATENÇÃO: Você possui faturas vencidas!" 
                            : "📅 Resumo de Faturas a Vencer";

                        string body = GenerateConsolidatedEmailBody(user.nm_nomeUsuario, pendingInvoices);

                        await _emailService.SendEmailAsync(user.nm_email, subject, body);
                        
                        emailsSent++;
                        details.Add($"Sent to {user.nm_email} with {pendingInvoices.Count} invoices");
                    }
                }

                log.ds_status = "Success";
                log.ds_message = $"Sent {emailsSent} invoice reminders.";
                log.ds_details = string.Join("; ", details);
                
                _context.integrationLogs.Add(log);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Processed", EmailsSent = emailsSent });
            }
            catch (Exception ex)
            {
                log.ds_status = "Error";
                log.ds_message = "Exception during processing";
                log.ds_details = ex.ToString();
                _context.integrationLogs.Add(log);
                await _context.SaveChangesAsync();
                return StatusCode(500, "Internal Server Error");
            }
        }

        private string GenerateConsolidatedEmailBody(string userName, List<(Invoice Invoice, DateTime DueDate, double DaysUntilDue)> invoices)
        {
            var sb = new StringBuilder();
            
            // Header
            sb.Append(@"
<!DOCTYPE html>
<html>
<head>
<style>
    body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #09090A; color: #E1E1E6; padding: 20px; margin: 0; }
    .container { max-width: 600px; margin: 0 auto; background-color: #121214; border-radius: 12px; overflow: hidden; border: 1px solid #29292E; box-shadow: 0 4px 20px rgba(0,0,0,0.5); }
    .header { background: linear-gradient(90deg, #00875F 0%, #00B37E 100%); padding: 30px 20px; text-align: center; }
    .header h1 { margin: 0; color: white; font-size: 24px; font-weight: bold; }
    .header p { margin: 5px 0 0; color: rgba(255,255,255,0.9); font-size: 14px; }
    .content { padding: 30px 20px; }
    .greeting { font-size: 18px; margin-bottom: 20px; color: #E1E1E6; }
    .card { background-color: #202024; border-radius: 8px; padding: 15px; margin-bottom: 12px; border-left: 4px solid #323238; display: flex; justify-content: space-between; align-items: center; }
    .card-content { flex: 1; }
    .card-title { font-weight: bold; font-size: 16px; color: #E1E1E6; display: block; margin-bottom: 4px; }
    .card-date { font-size: 12px; color: #7C7C8A; }
    .card-value { font-weight: bold; font-size: 16px; color: #E1E1E6; text-align: right; min-width: 100px; }
    
    .status-badge { display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 10px; font-weight: bold; text-transform: uppercase; margin-left: 8px; vertical-align: middle; }
    
    .status-overdue { border-color: #F75A68; } 
    .status-overdue .status-badge { background-color: rgba(247, 90, 104, 0.1); color: #F75A68; }
    .status-today { border-color: #FBA94C; }
    .status-today .status-badge { background-color: rgba(251, 169, 76, 0.1); color: #FBA94C; }
    .status-soon { border-color: #00B37E; }
    .status-soon .status-badge { background-color: rgba(0, 179, 126, 0.1); color: #00B37E; }

    .total-section { margin-top: 30px; border-top: 1px solid #323238; padding-top: 20px; text-align: right; }
    .total-label { color: #7C7C8A; font-size: 14px; }
    .total-value { font-size: 24px; font-weight: bold; color: #00B37E; }

    .btn { display: block; width: 100%; text-align: center; background-color: #00875F; color: white; padding: 16px 0; text-decoration: none; border-radius: 8px; font-weight: bold; margin-top: 30px; transition: background 0.2s; }
    .btn:hover { background-color: #00B37E; }
    
    .footer { padding: 20px; text-align: center; color: #7C7C8A; font-size: 12px; background-color: #09090A; border-top: 1px solid #202024; }
</style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Finance App</h1>
            <p>Resumo de Pagamentos</p>
        </div>
        <div class='content'>
            <div class='greeting'>Olá, <strong>" + userName + @"</strong>!</div>
            <p style='color: #C4C4CC; margin-bottom: 25px;'>Identificamos as seguintes faturas pendentes para este mês:</p>
");

            decimal total = 0;

            foreach (var item in invoices.OrderBy(x => x.DueDate))
            {
                total += item.Invoice.nr_valor;
                
                string statusClass = "status-soon";
                string statusLabel = "A Vencer";

                if (item.DaysUntilDue < 0)
                {
                    statusClass = "status-overdue";
                    statusLabel = "Vencida";
                }
                else if (item.DaysUntilDue == 0)
                {
                    statusClass = "status-today";
                    statusLabel = "Vence Hoje";
                }

                sb.Append($@"
            <div class='card {statusClass}'>
                <div class='card-content'>
                    <span class='card-title'>{item.Invoice.nm_descricao} <span class='status-badge'>{statusLabel}</span></span>
                    <span class='card-date'>Vencimento: {item.DueDate:dd/MM/yyyy}</span>
                </div>
                <div class='card-value'>
                    R$ {item.Invoice.nr_valor:N2}
                </div>
            </div>");
            }

            // Total and Footer
            sb.Append($@"
            <div class='total-section'>
                <span class='total-label'>Total a Pagar</span><br>
                <div class='total-value'>R$ {total:N2}</div>
            </div>

            <a href='http://localhost:5173/planning/invoices' class='btn'>Gerenciar Faturas</a>
        </div>
        <div class='footer'>
            <p>Este é um aviso automático. Por favor, não responda.</p>
            <p>&copy; 2024 Finance App</p>
        </div>
    </div>
</body>
</html>");

            return sb.ToString();
        }
    }
}

