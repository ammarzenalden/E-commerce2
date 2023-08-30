using E2.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E2.Models;

public class Product
{
    private readonly ApplicationDbContext _context;

    public Product(ApplicationDbContext context)
    {
        _context = context;
    }
    [Key]
    public int Id { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    public float Price { get; set; }
    [Required]
    public int Available { get; set; }
    [Required]
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    [JsonIgnore]
    public User? User { get; set; }
    [NotMapped]
    public float Rate { get; set; } = 0;
    [NotMapped]
    public int RateCount { get; set; } = 0;
    public bool Deleted { get; set; } = false;
    public void GetProductRate()
    {

        var rates = _context.ProductRates.Where(p => p.ProductId == Id).Select(p=>p.Rate);
        if (!rates.Any())
        {
            Rate = 0;
            RateCount = 0;
        }
        else
        {

            int count = rates.Count();
            Rate = (float)Queryable.Average(rates);
            RateCount = count;
        }
    }
    
}
