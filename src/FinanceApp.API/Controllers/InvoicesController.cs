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

                int emailsSent = 0;
                var details = new List<string>();

                foreach (var invoice in activeInvoices)
                {
                    // Check if already paid this month
                    bool paidThisMonth = invoice.dt_ultimoPagamento.HasValue &&
                                         invoice.dt_ultimoPagamento.Value.Month == today.Month &&
                                         invoice.dt_ultimoPagamento.Value.Year == today.Year;

                    if (paidThisMonth) continue;

                    // Calculate Due Date for THIS Month
                    // Handle February 30th etc -> Math.Min
                    int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
                    int day = Math.Min(invoice.nr_diaVencimento, daysInMonth);
                    var dueDate = new DateTime(today.Year, today.Month, day);

                    // If Due Date has passed, maybe still send? Or only BEFORE?
                    // Request says: "Start 5 days before... sending daily"
                    // So we send if (DueDate - Today) <= 5 days AND (DueDate >= Today OR DueDate < Today but still not paid? User said "daily alerting")
                    
                    // Let's alerting if DueDate is close (within 5 days) OR passed (overdue)
                    // Logic: If Today is >= (DueDate - 5 days)
                    
                    var daysUntilDue = (dueDate - today).TotalDays;

                    // If bill is for next month (e.g. today is 30th, bill is 5th), handle wrap?
                    // For simplicity, let's assume we look at current month's bill. 
                    // If today is 25th, bill is 30th -> starts alerting. 
                    // If today is 5th, bill is 5th -> alert.
                    // If today is 6th, bill was 5th -> alert overdue? "Start 5 days before... sending daily".
                    // Implies we keep sending until paid.

                    if (daysUntilDue <= 5 && invoice.Usuario != null && !string.IsNullOrEmpty(invoice.Usuario.nm_email))
                    {
                        // Send Email
                        string subject = daysUntilDue < 0 
                            ? $"🔴 ATRASADO: Sua fatura de {invoice.nm_descricao} venceu em {dueDate:dd/MM}!" 
                            : daysUntilDue == 0 
                                ? $"⚠️ HOJE: Sua fatura de {invoice.nm_descricao} vence hoje!" 
                                : $"📅 LEMBRETE: Sua fatura de {invoice.nm_descricao} vence em {daysUntilDue} dias";

                        string body = GenerateEmailBody(invoice, dueDate, daysUntilDue);

                        await _emailService.SendEmailAsync(invoice.Usuario.nm_email, subject, body);
                        
                        emailsSent++;
                        details.Add($"Sent to {invoice.Usuario.nm_email} for {invoice.nm_descricao}");
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

        private string GenerateEmailBody(Invoice i, DateTime dueDate, double daysUntilDue)
        {
            string statusColor = daysUntilDue < 0 ? "#F75A68" : (daysUntilDue == 0 ? "#FBA94C" : "#00B37E");
            string statusText = daysUntilDue < 0 ? "Vencida" : (daysUntilDue == 0 ? "Vence Hoje" : "A Vencer");

            return $@"
<!DOCTYPE html>
<html>
<head>
<style>
    body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #121214; color: #E1E1E6; padding: 20px; }}
    .container {{ max-width: 600px; margin: 0 auto; background-color: #202024; border-radius: 8px; overflow: hidden; border: 1px solid #323238; }}
    .header {{ background-color: #29292E; padding: 20px; text-align: center; border-bottom: 1px solid #323238; }}
    .content {{ padding: 40px 20px; text-align: center; }}
    .amount {{ font-size: 32px; font-weight: bold; color: {statusColor}; margin: 20px 0; }}
    .label {{ color: #7C7C8A; font-size: 14px; text-transform: uppercase; letter-spacing: 1px; }}
    .btn {{ display: inline-block; background-color: #00875F; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; margin-top: 20px; }}
    .footer {{ padding: 20px; text-align: center; color: #7C7C8A; font-size: 12px; border-top: 1px solid #323238; }}
</style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Finance App</h2>
        </div>
        <div class='content'>
            <p style='font-size: 18px;'>Olá, <strong>{i.Usuario?.nm_nomeUsuario}</strong>!</p>
            <p>Este é um lembrete sobre sua fatura:</p>
            
            <div style='margin-top: 30px;'>
                <div class='label'>Descrição</div>
                <h3 style='margin: 5px 0 20px 0;'>{i.nm_descricao}</h3>
                
                <div class='label'>Valor</div>
                <div class='amount'>R$ {i.nr_valor:N2}</div>
                
                <div class='label'>Vencimento</div>
                <p style='font-size: 18px; font-weight: bold;'>{dueDate:dd/MM/yyyy}</p>
                
                <div style='background-color: {statusColor}20; color: {statusColor}; display: inline-block; padding: 4px 12px; border-radius: 4px; font-weight: bold; margin-top: 10px;'>
                    {statusText}
                </div>
            </div>

            <p style='margin-top: 30px; color: #C4C4CC;'>
                Acesse o sistema para marcar como pago e parar de receber estes alertas este mês.
            </p>

            <a href='http://localhost:5173/invoices' class='btn'>Ir para o Sistema</a>
        </div>
        <div class='footer'>
            <p>Enviado automaticamente pelo Finance App Scheduler.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
