using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Data.Entities;

public enum RequestStatus
{
    [Display(Name = "Pending")]
    Pending = 0,

    [Display(Name = "Approved")]
    Approved = 1,

    [Display(Name = "In Progress")]
    InProgress = 2,

    [Display(Name = "Completed")]
    Completed = 3,

    [Display(Name = "Cancelled")]
    Cancelled = 4
}

public class ServiceRequest
{
    [Key]
    public int ServiceRequestId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Request Number")]
    public string RequestNumber { get; set; } = string.Empty;

    [Required]
    public int ContractId { get; set; }

    [ForeignKey("ContractId")]
    [Display(Name = "Contract")]
    public virtual Contract? Contract { get; set; }

    [Required]
    [StringLength(500)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Amount (USD)")]
    [Range(0.01, 999999.99)]
    public decimal AmountUSD { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Amount (ZAR)")]
    public decimal AmountZAR { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,4)")]
    [Display(Name = "Exchange Rate")]
    public decimal ExchangeRateUsed { get; set; }

    [Required]
    [Display(Name = "Status")]
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    [DataType(DataType.DateTime)]
    [Display(Name = "Request Date")]
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;

    [DataType(DataType.DateTime)]
    [Display(Name = "Completion Date")]
    public DateTime? CompletionDate { get; set; }

    [StringLength(1000)]
    [Display(Name = "Additional Notes")]
    public string Notes { get; set; } = string.Empty;
}