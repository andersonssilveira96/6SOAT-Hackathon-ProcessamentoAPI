using FIAPX.Processamento.Domain.Entities;

namespace FIAPX.Processamento.Domain.Interfaces.Repositories
{
    public interface IArquivoRepository
    {
        Task<Arquivo> CreateFile(Arquivo arquivoDto);
        Task<List<Arquivo>> GetAll();
        Task<Arquivo> GetById(Guid id);
        Task<Arquivo> Update(Arquivo arquivo);
    }
}
