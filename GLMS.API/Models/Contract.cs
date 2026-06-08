using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.API.Models
{
    public class Contract
    {
        [Key]
        public int ContractId { get; set; }

        [Required]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        public string ContractNumber { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } = "Draft"; // Draft, Active, Expired, OnHold

        public decimal ContractValue { get; set; }
        public string? PdfFilePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
    }
}