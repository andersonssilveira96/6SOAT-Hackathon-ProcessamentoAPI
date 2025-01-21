namespace FIAPX.Processamento.Infra.Data.UoW
{
    public interface IUnitOfWork
    {
        Task CommitAsync();        
    }
}
