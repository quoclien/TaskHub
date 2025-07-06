using System;
using System.Threading.Tasks;
using TaskHub.Api.Infrastructure.Data;

namespace TaskHub.Api.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ApplicationDbContext Context { get; }
        Task<int> SaveChangesAsync();
    }
}
