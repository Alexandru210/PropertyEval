using PropertyEval.Application.Interfaces;
using PropertyEval.Domain.Entities;
using PropertyEval.Infrastructure.Data;

namespace PropertyEval.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public Task<User> CreateUserAsync(string FirstName, string LastName, string Email, string PasswordHash, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
