using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledge.Application.Requests;
using PersonalKnowledge.Domain.Entities;

namespace PersonalKnowledge.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController(UserManager<User> userManager) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var user = await userManager.FindByIdAsync(userIdClaim.Value);
        if (user == null)
            return NotFound();

        return Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserResponse>> UpdateMe(UpdateUserRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var user = await userManager.FindByIdAsync(userIdClaim.Value);
        if (user == null)
            return NotFound();

        if (request.UserName != null)
            user.UserName = request.UserName;

        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            PhoneNumber = user.PhoneNumber
        });
    }
}
