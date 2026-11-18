using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProductOrderApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<IdentityUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    // 1. Register 
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var user = new IdentityUser { UserName = model.UserName, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        string roleToAssign = string.IsNullOrEmpty(model.Role) ? "User" : model.Role;

        await _userManager.AddToRoleAsync(user, roleToAssign);

        return Ok(new { message = $"User registered successfully as {roleToAssign}!" });
    }

    // 2. Login 
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.LoginID)
                   ?? await _userManager.FindByNameAsync(model.LoginID);

        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(new { message = "Invalid Email/Username or Password" });
        }

   
        var userRoles = await _userManager.GetRolesAsync(user);

       
        var token = await GenerateJwtToken(user, userRoles);

        return Ok(new { token = token });
    }

    // JWT Token 
    private async Task<string> GenerateJwtToken(IdentityUser user, IList<string> userRoles)
    {
       
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 16)
        {
            throw new InvalidOperationException("Jwt:Key is missing or too short in appsettings.json (must be at least 16 characters).");
        }

        var key = Encoding.UTF8.GetBytes(jwtKey);

        
        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.NameId, user.Id), 
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim(JwtRegisteredClaimNames.Sub, user.Id)
    };

        
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(3),
            Issuer = _configuration["Jwt:Issuer"] ?? "ProductOrderApi",
            Audience = _configuration["Jwt:Audience"] ?? "ProductOrderClient",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}