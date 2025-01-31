using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using FFMpegCore;
using FIAPX.Processamento.Application.DTOs;
using FIAPX.Processamento.Application.Factories;
using FIAPX.Processamento.Application.Services;
using FIAPX.Processamento.Domain.Entities;
using FIAPX.Processamento.Domain.Enum;
using FIAPX.Processamento.Domain.Interfaces.Repositories;
using FIAPX.Processamento.Domain.Producer;
using System.IO.Compression;

namespace FIAPX.Processamento.Application.UseCase
{
    public class ArquivoUseCase : IArquivoUseCase
    {
        private readonly IArquivoRepository _arquivoRepository;
        private readonly IMapper _mapper;
        private readonly IMessageBrokerProducer _messageBrokerProducer;
        private readonly IEmailService _emailService;
        private readonly string _s3BucketName = "fiapxarquivosbucket"; 
        private readonly IAmazonS3 _s3Client;

        public ArquivoUseCase(IArquivoRepository arquivoRepository, IMapper mapper, IMessageBrokerProducer messageBrokerProducer, IEmailService emailService, IAmazonS3 s3Client)
        {
            _arquivoRepository = arquivoRepository;
            _mapper = mapper;
            _messageBrokerProducer = messageBrokerProducer;
            _emailService = emailService;
            _s3Client = s3Client;
        }

        public async Task ProcessFile(ArquivoDto arquivoDto)
        {
            var arquivo = ArquivoFactory.Create(arquivoDto);

            try
            {
                arquivo.UpdateStatus(StatusEnum.Processando);

                await _arquivoRepository.CreateFile(arquivo);

                await _messageBrokerProducer.SendMessageAsync(arquivo);

                string localVideoPath = await DownloadFileFromS3(arquivoDto.Id.ToString(), arquivo.ContentType);

                string outputFolder = Path.Combine(Path.GetTempPath(), "splitted_videos");
                Directory.CreateDirectory(outputFolder);

                int intervalInSeconds = 5;

                List<string> snapshots = await GenerateSnapshots(localVideoPath, outputFolder, intervalInSeconds);

                string zipFilePath = await CreateZipFile(snapshots);

                string s3Key = $"{arquivoDto.Id.ToString()}/snapshots.zip";
                await UploadFileToS3(zipFilePath, s3Key);

                arquivo.UpdateStatus(StatusEnum.Processado);

                await _messageBrokerProducer.SendMessageAsync(arquivo);

                await _arquivoRepository.Update(arquivo);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Erro geral: {e.Message}");
                arquivo.UpdateStatus(StatusEnum.Erro);

                await _arquivoRepository.Update(arquivo);

                await _messageBrokerProducer.SendMessageAsync(arquivo);

                await _emailService.SendEmailAsync("andersonssilveira96@gmail.com", "cepol29137@halbov.com", "Teste envio", e.Message);
                
                throw;
            }
        }
        private async Task<string> CreateZipFile(List<string> snapshots)
        {
            string zipFilePath = Path.Combine(Path.GetTempPath(), "snapshots.zip");

            using (var zipFileStream = new FileStream(zipFilePath, FileMode.Create))
            using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
            {
                foreach (var snapshot in snapshots)
                {
                    var zipEntry = archive.CreateEntry(Path.GetFileName(snapshot), CompressionLevel.Fastest);

                    using (var entryStream = zipEntry.Open())
                    using (var snapshotStream = File.OpenRead(snapshot))
                    {
                        await snapshotStream.CopyToAsync(entryStream);
                    }
                }
            }

            return zipFilePath;
        }

        private async Task<string> DownloadFileFromS3(string s3Key, string contentType)
        {
            string localFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(s3Key));

            var request = new GetObjectRequest
            {
                BucketName = _s3BucketName,
                Key = $"{s3Key}/{s3Key}{GetVideoExtensionFromContentType(contentType)}",                
            };

            using (var response = await _s3Client.GetObjectAsync(request))
            using (var fileStream = File.Create(localFilePath))
            {
                await response.ResponseStream.CopyToAsync(fileStream);
            }

            return localFilePath;
        }

        private static async Task<List<string>> GenerateSnapshots(string videoPath, string outputDirectory, int intervalInSeconds)
        {
            string outputPattern = Path.Combine(outputDirectory, "snapshot_%03d.jpg");
            var generatedSnapshots = new List<string>();

            await FFMpegArguments
                .FromFileInput(videoPath)
                .OutputToFile(outputPattern, overwrite: true, options => options
                    .WithCustomArgument($"-vf fps=1/{intervalInSeconds}") 
                    .WithCustomArgument("-q:v 2")) 
                .ProcessAsynchronously();

            generatedSnapshots.AddRange(Directory.GetFiles(outputDirectory, "*.jpg"));

            return generatedSnapshots;
        }

        private async Task UploadFileToS3(string filePath, string s3Key)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _s3BucketName,
                Key = s3Key,
                FilePath = filePath
            };

            await _s3Client.PutObjectAsync(putRequest);
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
