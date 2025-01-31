using FIAPX.Processamento.Infra.Data.Context;
using System.Diagnostics.CodeAnalysis;

namespace FIAPX.Processamento.Infra.Data.UoW
{
    [ExcludeFromCodeCoverage]
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FIAPXContext _dataContext;
        private bool _disposed;

        public UnitOfWork(FIAPXContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task CommitAsync()
        {
            await _dataContext.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dataContext.Dispose();
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
