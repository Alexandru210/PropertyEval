using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Entities;
using PropertyEval.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace PropertyEval.Infrastructure.Services
{
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
            if (_context.Users.Any(u => u.Email == request.Email))
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            var passwordHash = HashPassword(request.Password);

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

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
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
        #endregion
    }
}
