using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Domain.Entities;
using PropertyEval.Infrastructure.Data;
using PropertyEval.Infrastructure.Security;

namespace PropertyEval.Infrastructure.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    #region Public Methods
    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        if (await _context.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var passwordHash = PasswordHasher.Hash(request.Password);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = email,
            PasswordHash = passwordHash,
            RoleId = SystemRoles.ClientId
        };

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new InvalidOperationException("User with this email already exists.", ex);
        }

        return new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            SystemRoles.Client
        );
    }

    public async Task<UserResponse> AuthenticateUserAsync(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.Name
        );
    }

    public async Task<IReadOnlyList<UserResponse>> GetEvaluatorsAsync(CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .Where(u => u.Role.Name == SystemRoles.Evaluator)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ThenBy(u => u.Email)
            .ToListAsync(cancellationToken);

        return users
            .Select(user => new UserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role.Name))
            .ToList();
    }

    public async Task<UserResponse> UpdateUserRoleAsync(int userId, string roleName, CancellationToken cancellationToken)
    {
        var normalizedRoleName = roleName.Trim();

        if (!SystemRoles.AssignableRoles.Contains(normalizedRoleName))
        {
            throw new InvalidOperationException("Role is not supported.");
        }

        var role = await _context.Roles
            .SingleOrDefaultAsync(r => r.Name == normalizedRoleName, cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException("Role does not exist.");
        }

        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        user.RoleId = role.Id;

        await _context.SaveChangesAsync(cancellationToken);

        return new UserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            role.Name
        );
    }
    #endregion

    #region Private Methods
    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
    }
    #endregion
}
