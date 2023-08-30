using System.ComponentModel.DataAnnotations;

namespace E2.DTO;

public class UserRegisterDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
    public string? HashPassword { get; set; }
}
