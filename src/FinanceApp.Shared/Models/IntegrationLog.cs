using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Shared.Models
{
    public class IntegrationLog
    {
        [Key]
        public int id_log { get; set; }
        
        public DateTime dt_log { get; set; } = DateTime.UtcNow;
        
        public string nm_integration { get; set; } = string.Empty; // e.g., "Scheduler"
        
        public string ds_status { get; set; } = string.Empty; // "Success", "Error"
        
        public string ds_message { get; set; } = string.Empty; // Summary
        
        public string ds_details { get; set; } = string.Empty; // Full details/JSON
        
        public string ds_ip { get; set; } = string.Empty; // Requester IP
    }
}
