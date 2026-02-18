using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotasApi.Services;

namespace NotasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TranscripcionController : ControllerBase
{
    private readonly ITranscripcionService _transcripcionService;
    private readonly ILogger<TranscripcionController> _logger;

    public TranscripcionController(
        ITranscripcionService transcripcionService,
        ILogger<TranscripcionController> logger)
    {
        _transcripcionService = transcripcionService;
        _logger = logger;
    }

    /// <summary>
    /// Transcribe un archivo de audio usando Whisper (OpenAI).
    /// Acepta webm, mp3, wav, etc.
    /// </summary>
    [HttpPost("audio")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<ActionResult<TranscribirResponse>> TranscribirAudio(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No se envió ningún archivo de audio" });

        var contentType = file.ContentType;
        if (string.IsNullOrEmpty(contentType))
            contentType = "audio/webm";

        try
        {
            using var stream = file.OpenReadStream();
            var texto = await _transcripcionService.TranscribirAsync(stream, contentType, ct);
            return Ok(new TranscribirResponse(texto));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("OpenAI"))
        {
            return StatusCode(500, new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al transcribir audio");
            return StatusCode(500, new { message = "Error al transcribir el audio", error = ex.Message });
        }
    }
}

public record TranscribirResponse(string Texto);
