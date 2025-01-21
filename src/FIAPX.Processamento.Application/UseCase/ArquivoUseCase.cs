using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using FIAPX.Processamento.Application.DTOs;
using FIAPX.Processamento.Application.Factories;
using FIAPX.Processamento.Domain.Enum;
using FIAPX.Processamento.Domain.Interfaces.Repositories;
using FIAPX.Processamento.Domain.Producer;

namespace FIAPX.Processamento.Application.UseCase
{
    public class ArquivoUseCase : IArquivoUseCase
    {
        private readonly IArquivoRepository _arquivoRepository;
        private readonly IMapper _mapper;
        private readonly IMessageBrokerProducer _messageBrokerProducer;
        private readonly string _s3BucketName = "fiapxarquivosbucket";

        public ArquivoUseCase(IArquivoRepository arquivoRepository, IMapper mapper, IMessageBrokerProducer messageBrokerProducer)
        {
            _arquivoRepository = arquivoRepository;
            _mapper = mapper;
            _messageBrokerProducer = messageBrokerProducer;
        }
        public async Task CreateFile(ArquivoDto arquivoDto, Stream stream)
        {
            try
            {
                var arquivo = ArquivoFactory.Create(arquivoDto);

                await _arquivoRepository.CreateFile(arquivo);

                using (var s3Client = new AmazonS3Client(RegionEndpoint.USEast1))
                {
                    await UploadFileAsync(s3Client, stream, _s3BucketName, arquivo.Id.ToString(), arquivo.ContentType);

                    await _messageBrokerProducer.SendMessageAsync(arquivo);
                };                          

                Console.WriteLine("Arquivo enviado com sucesso!");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Erro ao acessar o S3: {e.Message}");                
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro geral: {e.Message}");              
            }
        }

        private static async Task UploadFileAsync(IAmazonS3 s3Client, Stream fileStream, string bucketName, string keyName, string contentType)
        {            
            var uploadRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = keyName,
                InputStream = fileStream,
                ContentType = contentType
            };

            var response = await s3Client.PutObjectAsync(uploadRequest);

            Console.WriteLine($"Status do upload: {response.HttpStatusCode}");
        }

        public async Task<List<ArquivoDto>> GetAll()
        {
            var listaPedidos = await _arquivoRepository.GetAll();
            
            return _mapper.Map<List<ArquivoDto>>(listaPedidos);
        }

        public async Task<ArquivoDto> UpdateStatus(Guid id, int status)
        {
            var arquivo = await _arquivoRepository.GetById(id);

            if (arquivo is null)
                throw new Exception($"PedidoId {id} inválido");

            if (!Enum.IsDefined(typeof(StatusEnum), status))
                throw new Exception($"Status {status} inválido");

            arquivo.AtualizarStatus((StatusEnum)status);

            return _mapper.Map<ArquivoDto>(await _arquivoRepository.Update(arquivo));
        }
    }
}
