using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.API.Models
{
    public class ServiceRequest
    {
        [Key]
        public int ServiceRequestId { get; set; }

        [Required]
        public int ContractId { get; set; }

        [ForeignKey("ContractId")]
        public Contract? Contract { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public decimal ConvertedAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Declined, Completed

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletionDate { get; set; }
    }
}