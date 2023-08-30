using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E2.Models;

public class Comment
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    [JsonIgnore]
    public User? User { get; set; }
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    [JsonIgnore]
    public Product? Product { get; set; }
    [Required]
    public string? Text { get; set; }
    public object ToJson()
    {
        return new { UserId = UserId, ProductId = ProductId, Text = Text };
    }
}
