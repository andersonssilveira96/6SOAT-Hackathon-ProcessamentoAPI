using FIAPX.Processamento.Domain.Entities;
using FIAPX.Processamento.Domain.Interfaces.Repositories;
using FIAPX.Processamento.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAPX.Processamento.Infra.Data.Repositories
{
    public class ArquivoRepository : IArquivoRepository
    {
        private readonly FIAPXContext _context;
        public ArquivoRepository(FIAPXContext context)
        {
            _context = context;
        }

        public async Task<Arquivo> CreateFile(Arquivo arquivo)
        {
            if (arquivo is null)
            {
                throw new ArgumentNullException(nameof(arquivo));
            }

            _context.Arquivo.Add(arquivo);

            return arquivo;
        }

        public async Task<List<Arquivo>> GetAll() => await _context.Arquivo.ToListAsync();

        public async Task<Arquivo> GetById(Guid id) => await _context.Arquivo.FirstOrDefaultAsync(x => x.Id == id);        

        public async Task<Arquivo> Update(Arquivo arquivo)
        {
            var entry = _context.Entry(arquivo);

            _context.Arquivo.Update(entry.Entity);

            return arquivo;
        }
    }
}
