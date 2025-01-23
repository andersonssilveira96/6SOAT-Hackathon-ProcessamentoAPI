using FIAPX.Processamento.Application.DTOs;
using FIAPX.Processamento.Domain.Entities;

namespace FIAPX.Processamento.Application.Factories
{
    public static class ArquivoFactory
    {
        public static Arquivo Create(ArquivoDto arquivoDto)
        {
            return new Arquivo(arquivoDto.Id, arquivoDto.FileName, arquivoDto.ContentType, arquivoDto.Status, 0);
        }
    }
}
