using System.ComponentModel.DataAnnotations;

namespace E2.DTO;

public class UserDto
{
    [Required]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
    public string? DeviceToken { get; set; }
}
