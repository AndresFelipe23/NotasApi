namespace NotasApi.Services;

public interface IGoogleTasksService
{
    string GetAuthorizationUrl(Guid usuarioId);
    Task<(bool success, string? error)> ExchangeCodeForTokensAsync(string code, string state, Guid usuarioId);
    Task<(bool connected, string? email)> GetConnectionStatusAsync(Guid usuarioId);
    Task DisconnectAsync(Guid usuarioId);
    Task<IEnumerable<GoogleTaskListDto>> GetTaskListsAsync(Guid usuarioId);
    Task<IEnumerable<GoogleTaskDto>> GetTasksAsync(Guid usuarioId, string? taskListId = null);
    Task<bool> CompleteTaskAsync(Guid usuarioId, string taskListId, string taskId, bool completed);
    Task<bool> UpdateTaskAsync(Guid usuarioId, string taskListId, string taskId, string title, string? due);
    Task<bool> DeleteTaskAsync(Guid usuarioId, string taskListId, string taskId);
}

public record GoogleTaskListDto(string Id, string Title);

public record GoogleTaskDto(
    string Id,
    string Title,
    string? Notes,
    string Status,
    string? Due,
    string? Completed,
    string TaskListId,
    string TaskListTitle
);
