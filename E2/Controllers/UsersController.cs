using E2.Data;
using E2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using E2.configure;
using Microsoft.EntityFrameworkCore;
using E2.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using E2.Firebase;
using System.Reflection;

namespace E2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<User> _signInManager;
    public UsersController(ApplicationDbContext context,IConfiguration config, SignInManager<User> signInManager)
    {
        _config = config;
        _context = context;
        _signInManager = signInManager;
    }
    private int GetUserId()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        return int.Parse(userId!);
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(UserRegisterDto userDto)
    {
        Boolean hasNull=false;
        string theNull="";
        foreach (PropertyInfo property in userDto.GetType().GetProperties())
        {
            object value = property.GetValue(userDto);
            if (value == null)
            {
                hasNull = true;
                theNull=$"Property '{property.Name}' is null.";
            }
        }
        if (hasNull)
        {
            return BadRequest(new
            {
                success = true,
                message = theNull
            });
        };
        var currentUser = _context.Users.FirstOrDefault(x => x.Email!.ToLower() == userDto.Email!.ToLower());

        if (currentUser is null)
        {
            User user = new()
            {
                HashPassword = BCrypt.Net.BCrypt.HashPassword(userDto.HashPassword),
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                PhoneNumber = userDto.PhoneNumber,
                Email = userDto.Email
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                success = true,
                data = user.ToJson()
            }) ; 
        }
        else
        {
            return Conflict(new
            {
                success = false,
                message = "the user is existing"
            });
        }
    }
    [HttpPost("LogIn")]
    [AllowAnonymous]
    public ActionResult LogIn(UserDto user)
    {
        var currentUser = _context.Users.FirstOrDefault(x => x.Email!.ToLower() == user.Email!.ToLower());
        if (currentUser == null || !BCrypt.Net.BCrypt.Verify(user.Password,currentUser.HashPassword))
        {
            return Unauthorized(new { success = false ,message = "false email or password"});
        }
        DeviceToken deviceToken = new DeviceToken
        {
            UserId = currentUser.UserId,
            Token = user.DeviceToken
        };
        GenerateToken g = new GenerateToken(_context,_config);
        var tokenDto = g.GenerateTokens(currentUser.UserId);
        _context.DeviceTokens.Add(deviceToken);
        _context.SaveChanges();
        MobileMessagingClient mob = new MobileMessagingClient();
        string notMessage = "";
        try
        {
            mob.SendNotification(user.DeviceToken, "sucess", "welcome");
            notMessage = "success";

        }
        catch (Exception e)
        {
            notMessage = "somthing went wrong";
        }
        return Ok(new
        {
            success = true,
            message = "success",
            notification = notMessage,
            data = new
            {
                theToken = tokenDto.Token,
                refreshToken = tokenDto.RefreshToken,
                user = currentUser.ToJson()
            }
        });


    }
    [HttpGet("GetAllUsers")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> GetUsers()
    {
        if (_context.Users == null || !_context.Users.Any())
        {
            return Ok(new
            {
                success = true,
                message = "there is no users"
            });
        }
        List<User> alluser = await _context.Users.ToListAsync();
        List<object> values = new();
        foreach(var user in alluser)
        {
            values.Add(user.ToJson());
        }
        
        return Ok(new
        {
            success = true,
            data  = values
        });
    }
    [HttpGet("signin-google")]
    [AllowAnonymous]
    public IActionResult SignInWithGoogle()
    {
        var redirectUrl = Url.Action("GoogleCallback", "Users", null, Request.Scheme);
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return Challenge(properties, "Google");
    }
    [HttpGet("google-callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync("Google");

        if (authenticateResult.Succeeded)
        {
            // Get the user's information from the authentication result
            var user = authenticateResult.Principal;
            var existingUser = _context.Users.FirstOrDefault(x => x.Email!.ToLower() == user.FindFirstValue(ClaimTypes.Email));/*await _userManager.FindByEmailAsync(user.FindFirstValue(ClaimTypes.Email)!);*/

            if (existingUser is null)
            {
                // User doesn't exist, create a new User entity
                User newUser = new()
                {
                    FirstName = user.FindFirstValue(ClaimTypes.GivenName),
                    LastName = user.FindFirstValue(ClaimTypes.Surname),
                    Email = user.FindFirstValue(ClaimTypes.Email),
                    PhoneNumber = user.FindFirstValue(ClaimTypes.MobilePhone),
                    HashPassword = null
                    // Set other properties as needed
                };

                // Save the new user to the database
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                var theNewUser = _context.Users.Find(newUser);
                GenerateToken g = new GenerateToken(_context, _config);
                var tokenDto = g.GenerateTokens(theNewUser!.UserId);
                return Ok(new
                {
                    success = true,
                    theToken = tokenDto.Token,
                    refreshToken = tokenDto.RefreshToken,
                    data = theNewUser.ToJson()
                });
            }
            else
            {
                GenerateToken g = new GenerateToken(_context, _config);
                var tokenDto = g.GenerateTokens(existingUser.UserId);
                return Ok(new
                {
                    success = true,
                    theToken = tokenDto.Token,
                    refreshToken = tokenDto.RefreshToken,
                    data = existingUser.ToJson()
                });
            }
        }
        else
        {
            return BadRequest(new
            {
                success = false,
                message = "something went wrong"
            });
        }
    }

    [HttpPost("AddAdmin")]
    public async Task<IActionResult> AddAdmin(User user)
    {
        var currentUser = _context.Users.FirstOrDefault(x => x.Email!.ToLower() == user.Email!.ToLower());
        var userLogedIn = _context.Users.Find(GetUserId());
        if (userLogedIn!.Role == "Admin")
        {
            if (currentUser is null)
            {
                user.HashPassword = BCrypt.Net.BCrypt.HashPassword(user.HashPassword);
                user.Role = "Admin";
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    data = user.ToJson()
                });
            }
            else
            {
                return Ok(new
                {
                    success = false,
                    message = "the user is existing"
                });
            } 
        }
        else
        {
            return Unauthorized(new
            {
                success = false,
                message = "you are not admin"
            });
        }
    }
    [HttpDelete("DeleteUser/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new
            {
                success = false,
                message = "user not found"
            });
        }
        int userLogedInId = GetUserId();
        if (user.UserId == userLogedInId)
        {
            user.Deleted = true;
            _context.Users.Update(user);
            var deviceToken = _context.DeviceTokens.Where(p => p.UserId == user.UserId).ToList();
            if (deviceToken != null && deviceToken.Count > 0)
            {
                foreach (var device in deviceToken)
                {
                    _context.DeviceTokens.Remove(device);
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new
            {
                success = true,
                message = "deleted"
            });
        }
        else
        {
            return Unauthorized(new
            {
                success = false,
                message = "unAuthorized"
            });
        }

    }

    [HttpPost("LogOut")]
    public async Task<IActionResult> LogOut(string deviceToken)
    {
        var deviceLogedIn = await _context.DeviceTokens.FindAsync(deviceToken);
        
        if (deviceLogedIn is null)
        {
            return BadRequest(new
            {
                success = false,
                message = "something went wrong"
            });
        }
        else
        {
            
            _context.DeviceTokens.Remove(deviceLogedIn);
            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Logged out successfully"
            });
        }
    }

    [HttpGet("refreshToken")]
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {
        var refToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
        if(refToken is null || refToken.Expires < DateTime.UtcNow)
        {
            return BadRequest(new { success = false, message = "you need to login" });  
        }
        else
        {
            GenerateToken g = new GenerateToken(_context, _config);
            var tokenDto = g.GenerateTokens(refToken.UserId);
            return Ok(new
            {
                success = true,
                token = tokenDto.Token,
                refreshToken = tokenDto.RefreshToken
            });
        }
    }

}

