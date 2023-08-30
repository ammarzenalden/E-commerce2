using E2.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace E2.DTO;

public class CommentDto
{
    [Required]
    public int ProductId { get; set; }
    [Required]
    public string? Text { get; set; } 
}
