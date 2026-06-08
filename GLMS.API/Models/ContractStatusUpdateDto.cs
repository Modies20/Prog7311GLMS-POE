using System.ComponentModel.DataAnnotations;

namespace GLMS.API.Models
{
    public class ContractStatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Draft, Active, Expired, OnHold
    }
}
