using PropertyEval.Domain.Entities;

namespace PropertyEval.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(string FirstName, string LastName, string Email, string PasswordHash, CancellationToken cancellationToken);
    }
}
