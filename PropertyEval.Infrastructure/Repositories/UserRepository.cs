using PropertyEval.Domain.Entities;
using PropertyEval.Domain.Interfaces;
using PropertyEval.Infrastructure.Data;

namespace PropertyEval.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
