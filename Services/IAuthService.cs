using NotasApi.DTOs.Auth;

namespace NotasApi.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<bool> ValidateTokenAsync(string token);
    Task InvalidateTokenAsync(string token);
}
