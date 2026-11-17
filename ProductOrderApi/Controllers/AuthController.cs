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

    // 1. Register (အကောင့်အသစ်ဖွင့်ရန်)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var user = new IdentityUser { UserName = model.UserName, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // Role တစ်ခုခု ပါလာရင် ထည့်ပေးမယ်၊ မပါလာရင် Default "Customer" လို့ သတ်မှတ်မယ်
        string roleToAssign = string.IsNullOrEmpty(model.Role) ? "User" : model.Role;

        await _userManager.AddToRoleAsync(user, roleToAssign);

        return Ok(new { message = $"User registered successfully as {roleToAssign}!" });
    }

    // 2. Login (အကောင့်ဝင်ပြီး JWT Token ထုတ်ရန်)
    // 2. Login (အကောင့်ဝင်ပြီး JWT Token ထုတ်ရန်)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        // Email ဖြင့်ဖြစ်စေ၊ Username ဖြင့်ဖြစ်စေ ရှာဖွေနိုင်ရန်
        var user = await _userManager.FindByEmailAsync(model.LoginID)
                   ?? await _userManager.FindByNameAsync(model.LoginID);

        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized(new { message = "Invalid Email/Username or Password" });
        }

        // ⚠️ User ရဲ့ Roles များကို ဤနေရာတွင် ဆွဲထုတ်ပါ
        var userRoles = await _userManager.GetRolesAsync(user);

        // JWT Token ထုတ်ပေးခြင်း (Roles များကို ထည့်ပေးရန် ပို့လိုက်သည်)
        var token = await GenerateJwtToken(user, userRoles);

        return Ok(new { token = token });
    }

    // JWT Token ဖန်တီးသည့် Helper Method (Roles များကို Claims အဖြစ် ထည့်သွင်းခြင်း)
    private async Task<string> GenerateJwtToken(IdentityUser user, IList<string> userRoles)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

        // Base Claims များ
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        // ⚠️ User ရဲ့ Roles တစ်ခုချင်းစီကို Claims ထဲသို့ ထည့်ခြင်း
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(3), // Token သက်တမ်း ၃ နာရီ
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}