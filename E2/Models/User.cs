using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace E2.Models;

public class User : IdentityUser
{
    [Key]
    public int UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
    public string? HashPassword { get; set; }
    public bool Online { get; set; } = false;
    public bool Deleted { get; set; } = false;
    public string Role { get; set; } = "User";
    [JsonIgnore]
    public RefreshToken? RefreshToken { get; set; }

    public object ToJson()
    {
        return new { UserId = UserId, FirstName = FirstName, LastName = LastName, Email = Email, PhoneNumber = PhoneNumber };
    }
}
