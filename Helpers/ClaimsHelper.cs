using System.Security.Claims;

namespace NotasApi.Helpers;

public static class ClaimsHelper
{
    public static Guid GetUsuarioId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst("UsuarioId")?.Value 
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
            
        throw new UnauthorizedAccessException("Usuario no autenticado");
    }

    public static string GetCorreo(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value 
            ?? throw new UnauthorizedAccessException("Correo no encontrado en el token");
    }
}
