namespace NotasApi.Services;

public interface ITranscripcionService
{
    Task<string> TranscribirAsync(Stream audioStream, string contentType, CancellationToken ct = default);
}
