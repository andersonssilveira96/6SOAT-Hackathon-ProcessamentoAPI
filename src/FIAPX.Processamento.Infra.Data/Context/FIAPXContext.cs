using FIAPX.Processamento.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAPX.Processamento.Infra.Data.Context
{
    public sealed class FIAPXContext : DbContext
    {
        public FIAPXContext(DbContextOptions<FIAPXContext> options)
            : base(options)
        {
        }

        public DbSet<Arquivo> Arquivo { get; set; }
    }
}
