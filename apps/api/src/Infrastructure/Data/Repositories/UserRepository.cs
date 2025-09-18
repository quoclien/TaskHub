using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskHub.Api.Core.Interfaces;
using TaskHub.Api.Core.Interfaces.Repositories;
using TaskHub.Api.Infrastructure.Data;
using UserEntity = TaskHub.Api.Models.Entities.Users.User;

namespace TaskHub.Api.Infrastructure.Data.Repositories
{
    public class UserRepository : Repository<UserEntity>, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<UserEntity?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }
    }
}
