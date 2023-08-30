using System.ComponentModel.DataAnnotations;

namespace E2.DTO;
public class ProductDto 
{
    public string? Name { get; set; }
    public float? Price { get; set; }
    public int? Available { get; set; }
    public string? Description { get; set; }
}
