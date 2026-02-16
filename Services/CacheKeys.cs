namespace NotasApi.Services;

public static class CacheKeys
{
    // Árbol de carpetas - El más crítico para velocidad
    public static string CarpetaArbol(Guid usuarioId) => $"carpetas:arbol:{usuarioId}";
    
    // Dashboard - Notas rápidas
    public static string NotasRapidas(Guid usuarioId) => $"notasrapidas:todas:{usuarioId}";
    
    // Dashboard - Tareas pendientes
    public static string TareasPendientes(Guid usuarioId) => $"tareas:pendientes:{usuarioId}";
    
    // Invalidación de caché de carpetas (cuando se crea/elimina)
    public static string InvalidateCarpetas(Guid usuarioId) => $"carpetas:*:{usuarioId}";
    
    // Invalidación de dashboard
    public static string InvalidateDashboard(Guid usuarioId) => $"dashboard:*:{usuarioId}";
}
