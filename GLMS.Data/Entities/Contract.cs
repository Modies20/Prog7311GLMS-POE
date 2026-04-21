using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Data.Entities;

public enum ContractStatus
{
    [Display(Name = "Draft")]
    Draft = 0,

    [Display(Name = "Active")]
    Active = 1,

    [Display(Name = "Expired")]
    Expired = 2,

    [Display(Name = "On Hold")]
    OnHold = 3
}

public enum ServiceLevel
{
    [Display(Name = "Standard")]
    Standard = 0,

    [Display(Name = "Premium")]
    Premium = 1,

    [Display(Name = "Enterprise")]
    Enterprise = 2
}

public class Contract
{
    [Key]
    public int ContractId { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Contract Number")]
    public string ContractNumber { get; set; } = string.Empty;

    [Required]
    public int ClientId { get; set; }

    [ForeignKey("ClientId")]
    [Display(Name = "Client")]
    public virtual Client? Client { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Required]
    [Display(Name = "Status")]
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required]
    [Display(Name = "Service Level")]
    public ServiceLevel ServiceLevel { get; set; } = ServiceLevel.Standard;

    [StringLength(1000)]
    [Display(Name = "Terms & Conditions")]
    public string TermsAndConditions { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Agreement File")]
    public string FilePath { get; set; } = string.Empty;

    [Display(Name = "Created Date")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    // Calculated property (not stored in database)
    [NotMapped]
    [Display(Name = "Is Active")]
    public bool IsActive => Status == ContractStatus.Active &&
                            StartDate <= DateTime.Today &&
                            EndDate >= DateTime.Today;

    [NotMapped]
    [Display(Name = "Days Remaining")]
    public int DaysRemaining => EndDate > DateTime.Today ? (EndDate - DateTime.Today).Days : 0;
}
