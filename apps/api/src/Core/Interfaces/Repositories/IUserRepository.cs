using System;
using System.Threading.Tasks;
using TaskHub.Api.Models.Entities.Users;

namespace TaskHub.Api.Core.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
    }
}
