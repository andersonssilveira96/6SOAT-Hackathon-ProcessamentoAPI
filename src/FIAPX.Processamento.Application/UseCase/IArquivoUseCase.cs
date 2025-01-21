using FIAPX.Processamento.Application.DTOs;

namespace FIAPX.Processamento.Application.UseCase
{
    public interface IArquivoUseCase
    {
        Task CreateFile(ArquivoDto arquivoDto, Stream stream);
        Task<List<ArquivoDto>> GetAll();
        Task<ArquivoDto> UpdateStatus(Guid id, int status);
    }
}
