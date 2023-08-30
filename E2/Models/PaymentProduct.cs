using E2.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace E2.Models;

public class PaymentProduct
{
    
    [Key]
    public int Id { get; set; }
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    [JsonIgnore]
    public Product? Product { get; set; }
    public int PaymentId { get; set; }
    [ForeignKey("PaymentId")]
    [JsonIgnore]
    public Payment? Payment { get; set; }
    public int SellerId { get; set; }
    [ForeignKey("SellerId")]
    [JsonIgnore]
    public User? User { get; set; }
    public int BuyerId { get; set; }
    [ForeignKey("BuyerId")]
    [JsonIgnore]
    public User? Buyer { get; set; }
    public int Amount { get; set; }
    public float PriceOfSell { get; set; }
    public DateTime DateOfsell { get; set; }
}
