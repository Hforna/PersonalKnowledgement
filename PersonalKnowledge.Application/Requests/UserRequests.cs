using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledge.Application.Requests;

public class UpdateUserRequest
{
    public string? UserName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
    public string? PhoneNumber { get; set; }
}
