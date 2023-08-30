using E2.Data;
using E2.DTO;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace E2.configure;

public class GenerateToken
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;
    public GenerateToken(ApplicationDbContext context, IConfiguration config)
    {
        _config = config;
        _context = context;
    }
    public string GenerateApiToken(UserDto user)
    {
        var currentUser = _context.Users.FirstOrDefault(x => x.Email!.ToLower() == user.Email!.ToLower());
        var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.GetValue<string>("Authentication:SecretKey")!));
        var SigningCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        List<Claim> claims = new();
        claims.Add(new(JwtRegisteredClaimNames.Sub, currentUser!.UserId.ToString()));
        claims.Add(new(JwtRegisteredClaimNames.GivenName, currentUser!.FirstName!));
        claims.Add(new(JwtRegisteredClaimNames.FamilyName, currentUser!.LastName!));
        var token = new JwtSecurityToken(
            _config.GetValue<string>("Authentication:Issuer"),
            _config.GetValue<string>("Authentication:Audience"),
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            SigningCredentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
