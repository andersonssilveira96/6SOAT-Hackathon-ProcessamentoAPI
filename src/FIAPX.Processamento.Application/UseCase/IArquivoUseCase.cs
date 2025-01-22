using FIAPX.Processamento.Application.DTOs;

namespace FIAPX.Processamento.Application.UseCase
{
    public interface IArquivoUseCase
    {
        Task ProcessFile(ArquivoDto arquivoDto);
    }
}
