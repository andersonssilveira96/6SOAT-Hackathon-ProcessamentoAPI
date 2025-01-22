using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using FFMpegCore;
using FIAPX.Processamento.Application.DTOs;
using FIAPX.Processamento.Application.Factories;
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
        private static readonly RegionEndpoint BucketRegion = RegionEndpoint.USEast1; 
        private static readonly IAmazonS3 s3Client = new AmazonS3Client(BucketRegion);

        public ArquivoUseCase(IArquivoRepository arquivoRepository, IMapper mapper, IMessageBrokerProducer messageBrokerProducer)
        {
            _arquivoRepository = arquivoRepository;
            _mapper = mapper;
            _messageBrokerProducer = messageBrokerProducer;
        }
        public async Task ProcessFile(ArquivoDto arquivoDto)
        {
            try
            {
                var arquivo = ArquivoFactory.Create(arquivoDto);

                await _arquivoRepository.CreateFile(arquivo);

                // 1. Baixar o vídeo do S3
                string localVideoPath = await DownloadFileFromS3(arquivoDto.Id.ToString(), arquivo.ContentType);

                // 2. Dividir o vídeo usando FFmpeg
                string outputFolder = Path.Combine(Path.GetTempPath(), "splitted_videos");
                Directory.CreateDirectory(outputFolder);

                // Intervalo entre os snapshots (em segundos)
                int intervalInSeconds = 5;

                // Gerar snapshots
                List<string> snapshots = await GenerateSnapshots(localVideoPath, outputFolder, intervalInSeconds);

                // Fazer upload das snapshots para o S3
                foreach (var snapshot in snapshots)
                {
                    string s3Key = $"snapshots/{Path.GetFileName(snapshot)}"; // Caminho no bucket
                    await UploadFileToS3(snapshot, s3Key);
                }

                Console.WriteLine("Processo concluído com sucesso!");
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

        private async Task<string> DownloadFileFromS3(string s3Key, string contentType)
        {
            string localFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(s3Key));

            Console.WriteLine($"Baixando vídeo de '{s3Key}'...");
            var request = new GetObjectRequest
            {
                BucketName = _s3BucketName,
                Key = $"{s3Key}/{s3Key}{GetVideoExtensionFromContentType(contentType)}",                
            };

            using (var response = await s3Client.GetObjectAsync(request))
            using (var fileStream = File.Create(localFilePath))
            {
                await response.ResponseStream.CopyToAsync(fileStream);
            }

            Console.WriteLine($"Vídeo baixado para: {localFilePath}");
            return localFilePath;
        }

        private static async Task<List<string>> GenerateSnapshots(string videoPath, string outputDirectory, int intervalInSeconds)
        {
            Console.WriteLine($"Gerando snapshots a cada {intervalInSeconds} segundos...");

            // Monta o padrão de saída para os arquivos (snapshot_001.jpg, snapshot_002.jpg, etc.)
            string outputPattern = Path.Combine(outputDirectory, "snapshot_%03d.jpg");
            var generatedSnapshots = new List<string>();

            // Executa o comando FFmpeg para gerar os snapshots
            await FFMpegArguments
                .FromFileInput(videoPath)
                .OutputToFile(outputPattern, overwrite: true, options => options
                    .WithCustomArgument($"-vf fps=1/{intervalInSeconds}") // Frame por segundo baseado no intervalo
                    .WithCustomArgument("-q:v 2")) // Qualidade da imagem (2 é alta qualidade)
                .ProcessAsynchronously();

            // Adicionar os arquivos gerados à lista
            generatedSnapshots.AddRange(Directory.GetFiles(outputDirectory, "*.jpg"));

            Console.WriteLine($"Snapshots gerados: {generatedSnapshots.Count} arquivos.");
            return generatedSnapshots;
        }

        private async Task UploadFileToS3(string filePath, string s3Key)
        {
            Console.WriteLine($"Enviando '{filePath}' para '{s3Key}'...");
            var putRequest = new PutObjectRequest
            {
                BucketName = _s3BucketName,
                Key = s3Key,
                FilePath = filePath
            };

            await s3Client.PutObjectAsync(putRequest);
            Console.WriteLine($"Arquivo enviado: {s3Key}");
        }

        private static string GetVideoExtensionFromContentType(string contentType)
        {
            var videoMimeMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "video/mp4", ".mp4" },
                { "video/x-msvideo", ".avi" },
                { "video/x-matroska", ".mkv" },
                { "video/webm", ".webm" },
                { "video/ogg", ".ogv" },
                { "video/mpeg", ".mpeg" },
                { "video/quicktime", ".mov" },
                { "video/x-flv", ".flv" },
                { "video/3gpp", ".3gp" },
                { "video/3gpp2", ".3g2" }
            };

            return videoMimeMapping.TryGetValue(contentType, out var extension) ? extension : string.Empty;
        }
    }
}
