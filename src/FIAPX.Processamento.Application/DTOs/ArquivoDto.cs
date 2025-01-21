using FIAPX.Processamento.Domain.Enum;

namespace FIAPX.Processamento.Application.DTOs
{
    public class ArquivoDto
    {
        public Guid Id { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
        public StatusEnum Status { get; set; }
        public long UserId { get; set; }
    }
}
