using BCrypt.Net;
using E2.Data;
using E2.DTO;
using E2.Models;
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
    private string GenerateApiToken(int id)
    {
        var currentUser = _context.Users.Find(id);
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
    private RefreshToken GenerateRefreshToken(int id)
    {
        var currentUser = _context.Users.Find(id);
        // generate refresh token
        return new RefreshToken
        {
            UserId = currentUser.UserId,
            Expires = DateTime.UtcNow.AddDays(1),
            Token = Guid.NewGuid().ToString()
        };
    }
    public TokenDto GenerateTokens(int id)
    {
        var token = GenerateApiToken(id);
        var refreshToken = _context.RefreshTokens.FirstOrDefault(x => x.UserId == id);
        var newRefreshToken = new RefreshToken();
        if (refreshToken == null)
        { 
            newRefreshToken = GenerateRefreshToken(id);
            _context.RefreshTokens.Add(newRefreshToken);
            _context.SaveChanges();
        }
        else if(refreshToken.Expires < DateTime.UtcNow)
        {
            _context.RefreshTokens.Remove(refreshToken);
            newRefreshToken = GenerateRefreshToken(id);
            _context.RefreshTokens.Add(newRefreshToken);
            _context.SaveChanges();
        }
        else
        {
            newRefreshToken = refreshToken;
        }

        return new TokenDto(token,newRefreshToken.Token!);
    }

}
