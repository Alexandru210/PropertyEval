using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Entities;
using PropertyEval.Infrastructure.Data;
using System.Security.Cryptography;

namespace PropertyEval.Infrastructure.Services;

public class UserService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private const string PasswordHashVersion = "PBKDF2-SHA256";

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

        var passwordHash = HashPassword(request.Password);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = email,
            PasswordHash = passwordHash
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
            user.Email
        );
    }
    #endregion

    #region Private Methods
    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join(
            '.',
            PasswordHashVersion,
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException
            && sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 2601 or 2627);
    }
    #endregion
}
