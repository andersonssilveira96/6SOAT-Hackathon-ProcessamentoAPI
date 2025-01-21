using FIAPX.Processamento.Application.DTOs;
using FIAPX.Processamento.Application.UseCase;
using Microsoft.AspNetCore.Mvc;

namespace FIAPX.Processamento.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArquivoController : ControllerBase
    {       
        private readonly ILogger<ArquivoController> _logger;
        private readonly IArquivoUseCase _arquivoUseCase;
        public ArquivoController(ILogger<ArquivoController> logger, IArquivoUseCase arquivoUseCase)
        {
            _logger = logger;
            _arquivoUseCase = arquivoUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Nenhum arquivo foi enviado ou o arquivo está vazio.");
            }

            if (!file.ContentType.StartsWith("video/"))
            {
                return BadRequest("Apenas arquivos de vídeo são permitidos.");
            }

            if (file.Length > 52428800) // 50 MB
            {
                return BadRequest("O arquivo enviado é maior que o limite permitido (50 MB).");
            }

            var arquivo = new ArquivoDto
            {
                ContentType = file.ContentType,
                FileName = file.FileName              
            };

            using var stream = file.OpenReadStream();

            await _arquivoUseCase.CreateFile(arquivo, stream);

            return Ok(new { Message = "Upload realizado com sucesso!" });           
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var arquivos = await _arquivoUseCase.GetAll();

            return Ok(arquivos);
        }
    }
}
