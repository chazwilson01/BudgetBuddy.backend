using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.API.DTO;
using FinanceTracker.API.Models;
using FinanceTracker.API.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;


    public AuthController(AppDbContext context, IPasswordService passwordService, ITokenService tokenService)
    {
        _context = context;
        _passwordService = passwordService;
        _tokenService = tokenService;

    }


    // signup and login methods here

    [HttpPost("register")]
    public async Task<IActionResult> SignUp(UserDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest("User already exists.");
        }

        _passwordService.CreatePasswordHash(request.Password, out string passwordHash, out string passwordSalt);

        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);


        if (user == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return Unauthorized("Invalid email or password.");
        }
        string token = _tokenService.CreateToken(user);


        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // true in production (requires HTTPS)
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(new
        {
            message = "Login successful",
            user = new
            {
                email = user.Email,
                name = $"{user.FirstName} {user.LastName}",
                id = user.Id,
                hasBudget = user.HasBudget
            }
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new { email, name });
    }

    [HttpPut("update-budget-status")]
    public async Task<IActionResult> UpdateBudgetStatus([FromBody] UpdateBudgetStatusDto request)
    {
        // Get current user id from the token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("Invalid token");
        }

        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Update HasBudget field
        user.HasBudget = request.HasBudget;
        user.UpdatedAt = DateTime.UtcNow;

        // Save changes
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Budget status updated successfully",
            user = new
            {
                email = user.Email,
                name = $"{user.FirstName} {user.LastName}",
                id = user.Id,
                hasBudget = user.HasBudget
            }
        });
    }

    [HttpDelete("delete-user")]
    public async Task<IActionResult> DeleteUser()
    {
        // Get current user id from the token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("Invalid token");
        }
        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user == null)
        {
            return NotFound("User not found");
        }
        _context.Users.Remove(user);

        var budget = await _context.Budget.FirstOrDefaultAsync(b => b.userId == user.Id);
        if (budget != null)
        {
            _context.Budget.Remove(budget);
        }

        var categories = await _context.Categories.Where(c => c.UserId == user.Id).ToListAsync();
        if (categories.Count > 0)
        {
            _context.Categories.RemoveRange(categories);
        }

        await _context.SaveChangesAsync();
        return Ok("User deleted successfully");
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        // Get current user id from the token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized("Invalid token");
        }
        var user = await _context.Users.FindAsync(int.Parse(userId));
        if (user == null)
        {
            return NotFound("User not found");
        }
        if (!_passwordService.VerifyPassword(request.OldPassword, user.PasswordHash, user.PasswordSalt))
        {
            return Unauthorized("Invalid password");
        }
        _passwordService.CreatePasswordHash(request.NewPassword, out string passwordHash, out string passwordSalt);
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok("Password changed successfully");
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt");
        return Ok("Logged out successfully");
    }

}
