using System.ComponentModel.DataAnnotations;

namespace E2.DTO;

public class PaymentDto
{
    [Required]
    public int ProductId { get; set; }
    [Required]
    public int Amount { get; set; }
}
