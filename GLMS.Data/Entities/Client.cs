using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;


namespace GLMS.Data.Entities;

public class Client
{
    [Key]
    public int ClientId { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Company Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    [Display(Name = "Phone Number")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Region { get; set; } = string.Empty;

    [StringLength(20)]
    public string? TaxId { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
