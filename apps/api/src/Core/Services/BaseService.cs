using System;
using System.Threading.Tasks;
using TaskHub.Api.Core.Interfaces;

namespace TaskHub.Api.Core.Services
{
    public abstract class BaseService : IDisposable
    {
        protected readonly IUnitOfWork _unitOfWork;
        private bool _disposed = false;

        protected BaseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        protected async Task<int> SaveChangesAsync()
        {
            return await _unitOfWork.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _unitOfWork.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
