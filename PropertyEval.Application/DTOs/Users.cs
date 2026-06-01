namespace PropertyEval.Application.DTOs;

// Request DTOs
public record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
);

public record LoginUserRequest(
    string Email,
    string Password
);

public record UpdateUserRequest(
    int Id,
    string FirstName,
    string LastName,
    string Email
);

public class UpdateUserRoleRequest
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
}


// Response DTOs
public record UserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role
);

public record CreateUserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string Token
);

public record LoginUserResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string Token
);
