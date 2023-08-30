using System.ComponentModel.DataAnnotations;

namespace E2.DTO;

public class ProductRateDto
{
    [Required]
    public int ProductId { get; set; }
    [Range(1,5)]
    public int Rate { get; set; }
}
