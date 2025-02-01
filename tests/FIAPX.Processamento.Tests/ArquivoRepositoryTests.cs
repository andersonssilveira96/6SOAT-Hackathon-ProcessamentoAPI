using FIAPX.Processamento.Domain.Entities;
using FIAPX.Processamento.Domain.Enum;
using FIAPX.Processamento.Infra.Data.Context;
using FIAPX.Processamento.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FIAPX.Processamento.Tests
{
    public class ArquivoRepositoryTests
    {
        private readonly DbContextOptions<FIAPXContext> _options;

        public ArquivoRepositoryTests()
        {
            // Configura um banco de dados em memória
            _options = new DbContextOptionsBuilder<FIAPXContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Garante um banco novo a cada teste
                .Options;
        }

        [Fact]
        public async Task CreateFile_ShouldAddArquivo_ToDatabase()
        {
            using (var context = new FIAPXContext(_options))
            {
                var repository = new ArquivoRepository(context);
                var arquivo = new Arquivo(Guid.NewGuid(), "teste.mp4", "video/mp4", StatusEnum.Cadastrado, 0);

                await repository.CreateFile(arquivo);
                await context.SaveChangesAsync(); // Importante para persistir no banco

                var result = await context.Arquivo.FirstOrDefaultAsync(x => x.Id == arquivo.Id);

                Assert.NotNull(result);
                Assert.Equal("teste.mp4", result.FileName);
            }
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllArquivos()
        {
            using (var context = new FIAPXContext(_options))
            {
                context.Arquivo.Add(new Arquivo(Guid.NewGuid(), "file1.mp4", "video/mp4", StatusEnum.Cadastrado, 0));
                context.Arquivo.Add(new Arquivo(Guid.NewGuid(), "file2.mp4", "video/mp4", StatusEnum.Cadastrado, 0));
                await context.SaveChangesAsync();

                var repository = new ArquivoRepository(context);
                var result = await repository.GetAll();

                Assert.Equal(2, result.Count);
            }
        }

        [Fact]
        public async Task GetById_ShouldReturnArquivo_WhenIdExists()
        {
            using (var context = new FIAPXContext(_options))
            {
                var arquivo = new Arquivo(Guid.NewGuid(), "file.mp4", "video/mp4", StatusEnum.Cadastrado, 0);
                context.Arquivo.Add(arquivo);
                await context.SaveChangesAsync();

                var repository = new ArquivoRepository(context);
                var result = await repository.GetById(arquivo.Id);

                Assert.NotNull(result);
                Assert.Equal(arquivo.Id, result.Id);
            }
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            using (var context = new FIAPXContext(_options))
            {
                var repository = new ArquivoRepository(context);
                var result = await repository.GetById(Guid.NewGuid());

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task Update_ShouldModifyArquivo()
        {
            using (var context = new FIAPXContext(_options))
            {
                var arquivo = new Arquivo(Guid.NewGuid(), "file.mp4", "video/mp4", StatusEnum.Cadastrado, 0);
                context.Arquivo.Add(arquivo);
                await context.SaveChangesAsync();

                var repository = new ArquivoRepository(context);
                arquivo.UpdateStatus(StatusEnum.Processado);
                await repository.Update(arquivo);
                await context.SaveChangesAsync();

                var updated = await context.Arquivo.FirstOrDefaultAsync(x => x.Id == arquivo.Id);
                Assert.NotNull(updated);
                Assert.Equal(StatusEnum.Processado, updated.Status);
            }
        }

        [Fact]
        public async Task CreateFile_ShouldReturnException_WhenNull()
        {
            // Arrange
            using (var context = new FIAPXContext(_options))
            {
                var repository = new ArquivoRepository(context);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentNullException>(async () => await repository.CreateFile((Arquivo)null));
            }           
        }
    }
}
