using FIAPX.Processamento.Infra.Data.UoW;

namespace FIAPX.Processamento.Api.Middleware
{
    public class UnitOfWorkMiddleware : IMiddleware
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UnitOfWorkMiddleware> _logger;

        public UnitOfWorkMiddleware(IUnitOfWork unitOfWork, ILogger<UnitOfWorkMiddleware> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception)
            {
                _logger.LogError("An error occurred while saving the changes to the database.");
                throw;
            }
        }
    }
}
