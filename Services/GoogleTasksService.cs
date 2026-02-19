using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NotasApi.Models;
using NotasApi.Repositories;

namespace NotasApi.Services;

public class GoogleTasksService : IGoogleTasksService
{
    private readonly IConfiguration _configuration;
    private readonly IIntegracionGoogleRepository _integracionRepo;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v2/userinfo";
    private const string TasksApiBase = "https://tasks.googleapis.com/tasks/v1";

    public GoogleTasksService(
        IConfiguration configuration,
        IIntegracionGoogleRepository integracionRepo,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _integracionRepo = integracionRepo;
        _httpClientFactory = httpClientFactory;
    }

    private (string clientId, string clientSecret, string redirectUri) GetGoogleConfig()
    {
        var clientId = _configuration["Google:ClientId"] ?? throw new InvalidOperationException("Google:ClientId no configurado");
        var clientSecret = _configuration["Google:ClientSecret"] ?? throw new InvalidOperationException("Google:ClientSecret no configurado");
        var redirectUri = _configuration["Google:RedirectUri"] ?? throw new InvalidOperationException("Google:RedirectUri no configurado");
        return (clientId, clientSecret, redirectUri);
    }

    public string GetAuthorizationUrl(Guid usuarioId)
    {
        var (clientId, _, redirectUri) = GetGoogleConfig();
        var state = Convert.ToBase64String(Encoding.UTF8.GetBytes(usuarioId.ToString()));
        var scopes = string.Join(" ", new[]
        {
            "https://www.googleapis.com/auth/tasks",
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/userinfo.profile"
        });

        var query = string.Join("&", new[]
        {
            $"client_id={Uri.EscapeDataString(clientId)}",
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}",
            "response_type=code",
            $"scope={Uri.EscapeDataString(scopes)}",
            $"state={Uri.EscapeDataString(state)}",
            "access_type=offline",
            "prompt=consent"
        });

        return $"{AuthEndpoint}?{query}";
    }

    public async Task<(bool success, string? error)> ExchangeCodeForTokensAsync(string code, string state, Guid usuarioId)
    {
        try
        {
            var (clientId, clientSecret, redirectUri) = GetGoogleConfig();

            // Validar state (CSRF)
            if (!Guid.TryParse(Encoding.UTF8.GetString(Convert.FromBase64String(state)), out var stateUserId) || stateUserId != usuarioId)
                return (false, "Estado de autorización inválido");

            var httpClient = _httpClientFactory.CreateClient();
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            });

            var response = await httpClient.PostAsync(TokenEndpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Log del error para debugging
                Console.WriteLine($"Error de Google OAuth: {response.StatusCode} - {responseBody}");
                return (false, $"Error de Google: {responseBody}");
            }

            // Configurar opciones de deserialización para manejar nombres de propiedades
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseBody, jsonOptions);
            
            // Verificar si hay error en la respuesta
            if (!string.IsNullOrEmpty(tokenResponse?.Error))
            {
                var errorMsg = $"Error de Google: {tokenResponse.Error}";
                if (!string.IsNullOrEmpty(tokenResponse.ErrorDescription))
                    errorMsg += $" - {tokenResponse.ErrorDescription}";
                Console.WriteLine($"{errorMsg}. Respuesta completa: {responseBody}");
                return (false, errorMsg);
            }
            
            if (tokenResponse?.AccessToken == null)
            {
                Console.WriteLine($"No se recibió AccessToken. Respuesta completa: {responseBody}");
                return (false, $"No se recibió el token de acceso de Google. Respuesta: {responseBody}");
            }

            // Si no hay RefreshToken, intentar obtenerlo de una integración existente
            string refreshToken = tokenResponse.RefreshToken ?? "";
            if (string.IsNullOrEmpty(refreshToken))
            {
                var integracionExistente = await _integracionRepo.ObtenerPorUsuarioAsync(usuarioId);
                if (integracionExistente != null && !string.IsNullOrEmpty(integracionExistente.RefreshToken))
                {
                    refreshToken = integracionExistente.RefreshToken;
                    Console.WriteLine("Usando RefreshToken existente de la base de datos");
                }
                else
                {
                    Console.WriteLine($"No se recibió RefreshToken y no hay uno existente. Respuesta: {responseBody}");
                    return (false, "No se recibió el token de refresco. Por favor, desconecta y vuelve a conectar.");
                }
            }

            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            // Obtener info del usuario de Google
            var (googleEmail, googleName) = await GetUserInfoAsync(tokenResponse.AccessToken);

            var integracion = new IntegracionGoogle
            {
                UsuarioId = usuarioId,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = refreshToken,
                TokenExpiresAt = expiresAt,
                GoogleEmail = googleEmail,
                GoogleName = googleName
            };

            await _integracionRepo.UpsertAsync(integracion);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private async Task<(string? email, string? name)> GetUserInfoAsync(string accessToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync(UserInfoEndpoint);
            if (!response.IsSuccessStatusCode) return (null, null);

            var json = await response.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(json);
            return (userInfo?.Email, userInfo?.Name);
        }
        catch
        {
            return (null, null);
        }
    }

    public async Task<(bool connected, string? email)> GetConnectionStatusAsync(Guid usuarioId)
    {
        var integracion = await _integracionRepo.ObtenerPorUsuarioAsync(usuarioId);
        return (integracion != null, integracion?.GoogleEmail);
    }

    public async Task DisconnectAsync(Guid usuarioId)
    {
        await _integracionRepo.DesconectarAsync(usuarioId);
    }

    public async Task<IEnumerable<GoogleTaskListDto>> GetTaskListsAsync(Guid usuarioId)
    {
        var accessToken = await GetValidAccessTokenAsync(usuarioId);
        if (accessToken == null) return Enumerable.Empty<GoogleTaskListDto>();

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync($"{TasksApiBase}/users/@me/lists");
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error obteniendo listas de Google Tasks: {response.StatusCode} - {errorBody}");
            return Enumerable.Empty<GoogleTaskListDto>();
        }

        var json = await response.Content.ReadAsStringAsync();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<GoogleTaskListsResponse>(json, jsonOptions);
        return (data?.Items ?? []).Select(x => new GoogleTaskListDto(x.Id, x.Title ?? "Sin nombre"));
    }

    public async Task<IEnumerable<GoogleTaskDto>> GetTasksAsync(Guid usuarioId, string? taskListId = null)
    {
        var accessToken = await GetValidAccessTokenAsync(usuarioId);
        if (accessToken == null)
        {
            Console.WriteLine($"No se pudo obtener access token válido para usuario {usuarioId}");
            return Enumerable.Empty<GoogleTaskDto>();
        }

        var lists = await GetTaskListsAsync(usuarioId);
        Console.WriteLine($"Se encontraron {lists.Count()} listas de tareas en Google Tasks");
        
        if (!lists.Any())
        {
            Console.WriteLine("No hay listas de tareas en Google Tasks");
            return Enumerable.Empty<GoogleTaskDto>();
        }

        var listIds = taskListId != null ? [taskListId] : lists.Select(l => l.Id).ToList();
        var allTasks = new List<GoogleTaskDto>();
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        foreach (var list in lists.Where(l => listIds.Contains(l.Id)))
        {
            Console.WriteLine($"Obteniendo tareas de lista: {list.Title} ({list.Id})");
            var response = await httpClient.GetAsync($"{TasksApiBase}/lists/{list.Id}/tasks?showCompleted=true&showHidden=true");
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error obteniendo tareas de lista {list.Id}: {response.StatusCode} - {errorBody}");
                continue;
            }

            var json = await response.Content.ReadAsStringAsync();
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<GoogleTasksResponse>(json, jsonOptions);
            
            var tasksInList = data?.Items ?? [];
            Console.WriteLine($"Se encontraron {tasksInList.Count} tareas en la lista {list.Title}");
            
            foreach (var item in tasksInList)
            {
                allTasks.Add(new GoogleTaskDto(
                    item.Id ?? "",
                    item.Title ?? "",
                    item.Notes,
                    item.Status ?? "needsAction",
                    item.Due,
                    item.Completed,
                    list.Id,
                    list.Title
                ));
            }
        }

        Console.WriteLine($"Total de tareas obtenidas: {allTasks.Count}");
        return allTasks;
    }

    public async Task<bool> CompleteTaskAsync(Guid usuarioId, string taskListId, string taskId, bool completed)
    {
        var accessToken = await GetValidAccessTokenAsync(usuarioId);
        if (accessToken == null)
        {
            Console.WriteLine($"No se pudo obtener access token válido para usuario {usuarioId}");
            return false;
        }

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Obtener la tarea actual para actualizar solo el status
        var getTaskResponse = await httpClient.GetAsync($"{TasksApiBase}/lists/{taskListId}/tasks/{taskId}");
        if (!getTaskResponse.IsSuccessStatusCode)
        {
            var errorBody = await getTaskResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Error obteniendo tarea de Google: {getTaskResponse.StatusCode} - {errorBody}");
            return false;
        }

        var taskJson = await getTaskResponse.Content.ReadAsStringAsync();
        var deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var taskData = JsonSerializer.Deserialize<GoogleTaskItem>(taskJson, deserializeOptions);
        
        if (taskData == null)
        {
            Console.WriteLine("No se pudo deserializar la tarea de Google");
            return false;
        }

        // Actualizar el status usando PATCH
        // Google Tasks API requiere que se envíe solo el campo que se quiere actualizar
        var updateData = new Dictionary<string, object>();
        
        if (completed)
        {
            updateData["status"] = "completed";
            // Si no tiene fecha de completado, agregarla
            if (string.IsNullOrEmpty(taskData.Completed))
            {
                updateData["completed"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
        }
        else
        {
            updateData["status"] = "needsAction";
            // Limpiar fecha de completado
            updateData["completed"] = null;
        }

        var serializeOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var updateJson = JsonSerializer.Serialize(updateData, serializeOptions);
        var content = new StringContent(updateJson, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Patch, $"{TasksApiBase}/lists/{taskListId}/tasks/{taskId}")
        {
            Content = content
        };
        var patchResponse = await httpClient.SendAsync(request);
        if (!patchResponse.IsSuccessStatusCode)
        {
            var errorBody = await patchResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Error actualizando tarea de Google: {patchResponse.StatusCode} - {errorBody}");
            return false;
        }

        Console.WriteLine($"Tarea de Google {taskId} marcada como {(completed ? "completada" : "pendiente")}");
        return true;
    }

    public async Task<bool> UpdateTaskAsync(Guid usuarioId, string taskListId, string taskId, string title, string? due)
    {
        var accessToken = await GetValidAccessTokenAsync(usuarioId);
        if (accessToken == null)
        {
            Console.WriteLine($"No se pudo obtener access token válido para usuario {usuarioId}");
            return false;
        }

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var updateData = new Dictionary<string, object>
        {
            ["title"] = title
        };

        if (!string.IsNullOrEmpty(due))
        {
            // Convertir fecha al formato RFC3339 que espera Google Tasks
            // Google Tasks espera formato: YYYY-MM-DDTHH:mm:ss.sssZ o solo YYYY-MM-DD
            // Si viene con hora, convertir a UTC; si es solo fecha, usar formato simple
            if (DateTimeOffset.TryParse(due, out var fechaParsed))
            {
                // Google Tasks acepta solo la fecha o fecha con hora en UTC
                updateData["due"] = fechaParsed.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            else if (due.Length == 10) // Solo fecha YYYY-MM-DD
            {
                updateData["due"] = due;
            }
            else
            {
                updateData["due"] = due; // Intentar tal cual
            }
        }
        else
        {
            updateData["due"] = null;
        }

        var serializeOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var updateJson = JsonSerializer.Serialize(updateData, serializeOptions);
        var content = new StringContent(updateJson, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Patch, $"{TasksApiBase}/lists/{taskListId}/tasks/{taskId}")
        {
            Content = content
        };
        var patchResponse = await httpClient.SendAsync(request);
        
        if (!patchResponse.IsSuccessStatusCode)
        {
            var errorBody = await patchResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Error actualizando tarea de Google: {patchResponse.StatusCode} - {errorBody}");
            return false;
        }

        Console.WriteLine($"Tarea de Google {taskId} actualizada exitosamente");
        return true;
    }

    public async Task<bool> DeleteTaskAsync(Guid usuarioId, string taskListId, string taskId)
    {
        var accessToken = await GetValidAccessTokenAsync(usuarioId);
        if (accessToken == null)
        {
            Console.WriteLine($"No se pudo obtener access token válido para usuario {usuarioId}");
            return false;
        }

        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var deleteResponse = await httpClient.DeleteAsync($"{TasksApiBase}/lists/{taskListId}/tasks/{taskId}");
        
        if (!deleteResponse.IsSuccessStatusCode)
        {
            var errorBody = await deleteResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Error eliminando tarea de Google: {deleteResponse.StatusCode} - {errorBody}");
            return false;
        }

        Console.WriteLine($"Tarea de Google {taskId} eliminada exitosamente");
        return true;
    }

    private async Task<string?> GetValidAccessTokenAsync(Guid usuarioId)
    {
        var integracion = await _integracionRepo.ObtenerPorUsuarioAsync(usuarioId);
        if (integracion == null)
        {
            Console.WriteLine($"No se encontró integración de Google para usuario {usuarioId}");
            return null;
        }

        // Si el token expira en menos de 5 minutos, refrescar
        if (integracion.TokenExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(5))
        {
            Console.WriteLine($"Token de Google expirando pronto, refrescando...");
            var newToken = await RefreshAccessTokenAsync(integracion.RefreshToken);
            if (newToken == null)
            {
                Console.WriteLine($"Error al refrescar token de Google para usuario {usuarioId}");
                return null;
            }

            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(3600);
            await _integracionRepo.ActualizarTokensAsync(usuarioId, newToken, expiresAt);
            Console.WriteLine($"Token de Google refrescado exitosamente");
            return newToken;
        }

        Console.WriteLine($"Usando token de Google existente (expira en {(integracion.TokenExpiresAt - DateTimeOffset.UtcNow).TotalMinutes:F1} minutos)");
        return integracion.AccessToken;
    }

    private async Task<string?> RefreshAccessTokenAsync(string refreshToken)
    {
        var (clientId, clientSecret, _) = GetGoogleConfig();
        var httpClient = _httpClientFactory.CreateClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        });

        var response = await httpClient.PostAsync(TokenEndpoint, content);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(json, jsonOptions);
        return tokenResponse?.AccessToken;
    }

    private class GoogleTokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("error")]
        public string? Error { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
    }

    private class GoogleUserInfo
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
    }

    private class GoogleTaskListsResponse
    {
        public List<GoogleTaskListItem>? Items { get; set; }
    }

    private class GoogleTaskListItem
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
    }

    private class GoogleTasksResponse
    {
        public List<GoogleTaskItem>? Items { get; set; }
    }

    private class GoogleTaskItem
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }
        public string? Due { get; set; }
        public string? Completed { get; set; }
    }
}
