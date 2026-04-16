using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledge.Application.Requests;
using PersonalKnowledge.Application.Responses;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<User> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    ITokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var userExists = await userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
            return BadRequest(new AuthResponse { Succeeded = false, Message = "User already exists" });

        var user = new User
        {
            Email = request.Email,
            UserName = request.UserName ?? request.Email,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new AuthResponse 
            { 
                Succeeded = false, 
                Message = "User creation failed", 
                Errors = result.Errors.Select(e => e.Description) 
            });

        // Add default role
        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("User"));

        await userManager.AddToRoleAsync(user, "User");

        return Ok(new AuthResponse { Succeeded = true, Message = "User created successfully" });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new AuthResponse { Succeeded = false, Message = "Invalid credentials" });

        var userRoles = await userManager.GetRolesAsync(user);
        var token = tokenService.GenerateJwtToken(user.Id.ToString(), user.UserName!, userRoles);

        user.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return Ok(new AuthResponse
        {
            Succeeded = true,
            Token = token,
            Message = "Login successful"
        });
    }

    [HttpPost("add-role")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            return Ok(new { message = $"Role {roleName} created successfully" });
        }
        return BadRequest(new { message = "Role already exists" });
    }

    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole(string email, string roleName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null) return NotFound(new { message = "User not found" });

        if (!await roleManager.RoleExistsAsync(roleName))
            return BadRequest(new { message = "Role does not exist" });

        await userManager.AddToRoleAsync(user, roleName);
        return Ok(new { message = $"Role {roleName} assigned to {email}" });
    }
}
