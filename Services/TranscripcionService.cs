using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace NotasApi.Services;

public class TranscripcionService : ITranscripcionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<TranscripcionService> _logger;

    public TranscripcionService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<TranscripcionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<string> TranscribirAsync(Stream audioStream, string contentType, CancellationToken ct = default)
    {
        var apiKey = _config["OpenAI:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException(
                "OpenAI:ApiKey no configurado. Añade 'OpenAI': { 'ApiKey': 'sk-...' } en appsettings.json");

        using var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        client.Timeout = TimeSpan.FromMinutes(2);

        var isWebm = contentType?.Contains("webm", StringComparison.OrdinalIgnoreCase) == true;
        var whisperContentType = isWebm ? "audio/webm" : contentType;
        var extension = isWebm ? "webm" : (contentType?.Contains("mp4") == true ? "mp4" : "webm");
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(audioStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(whisperContentType ?? "audio/webm");
        content.Add(streamContent, "file", $"audio.{extension}");
        content.Add(new StringContent("whisper-1"), "model");
        content.Add(new StringContent("es"), "language");
        content.Add(new StringContent("json"), "response_format");
        content.Add(new StringContent("Transcripción de reunión. Solo transcribe cuando alguien hable. No repitas frases."), "prompt");

        var response = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", content, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorMsg = json;
            try
            {
                using var errDoc = JsonDocument.Parse(json);
                if (errDoc.RootElement.TryGetProperty("error", out var err) && err.TryGetProperty("message", out var msg))
                    errorMsg = msg.GetString() ?? json;
            }
            catch { /* usar json raw */ }
            _logger.LogWarning("Whisper API error {Status}: {Response}", response.StatusCode, json);
            throw new HttpRequestException($"Whisper API: {response.StatusCode} - {errorMsg}");
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var raw = root.TryGetProperty("text", out var textProp) ? textProp.GetString() ?? "" : "";
        return FiltrarAlucinaciones(raw);
    }

    private static string FiltrarAlucinaciones(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return "";
        var t = texto.Trim();
        var ignorar = new[] {
            "Subtítulos realizados por la comunidad de Amara.org",
            "Subtítulos realizados por la comunidad de Amara",
            "amara.org",
        };
        foreach (var frase in ignorar)
        {
            if (string.IsNullOrEmpty(frase)) continue;
            while (t.IndexOf(frase, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var i = t.IndexOf(frase, StringComparison.OrdinalIgnoreCase);
                t = t.Remove(i, frase.Length).Trim();
            }
        }
        t = System.Text.RegularExpressions.Regex.Replace(t, @"\s+", " ");
        var palabras = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var resultado = new List<string>();
        var anterior = "";
        var repeticiones = 0;
        foreach (var p in palabras)
        {
            if (p.Equals(anterior, StringComparison.OrdinalIgnoreCase))
            {
                repeticiones++;
                if (repeticiones <= 2) resultado.Add(p);
                continue;
            }
            anterior = p;
            repeticiones = 1;
            resultado.Add(p);
        }
        return string.Join(" ", resultado).Trim();
    }
}
