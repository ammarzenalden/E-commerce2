using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E2.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }
    public float TotalPrice { get; set; }
    public int NumberOfProduct { get; set; }
    public DateTime DateOfSell { get; set; }
}
