using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NotasApi.DTOs.Auth;
using NotasApi.Models;
using NotasApi.Repositories;

namespace NotasApi.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly int _jwtExpirationMinutes;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        ICacheService cacheService,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _cacheService = cacheService;
        _configuration = configuration;
        
        _jwtSecret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado");
        _jwtIssuer = configuration["Jwt:Issuer"] ?? "AnotaAPI";
        _jwtExpirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "1440"); // 24 horas por defecto
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Buscar usuario por correo
        var usuario = await _usuarioRepository.ObtenerPorCorreoAsync(request.Correo);
        if (usuario == null)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        // No revelar si la cuenta existe; mismo mensaje que credenciales incorrectas
        if (!usuario.EsActivo)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        // Verificar contraseña con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        // Actualizar último acceso
        await _usuarioRepository.ActualizarUltimoAccesoAsync(usuario.Id);

        // Generar token JWT
        var token = GenerateJwtToken(usuario);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Usuario = new UsuarioInfo
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                FotoPerfilUrl = usuario.FotoPerfilUrl
            }
        };
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        // Verificar si el correo ya existe
        var usuarioExistente = await _usuarioRepository.ObtenerPorCorreoAsync(request.Correo);
        if (usuarioExistente != null)
            throw new InvalidOperationException("El correo ya está registrado");

        // Crear nuevo usuario
        var nuevoUsuario = new Usuario
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Correo = request.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EsActivo = true
        };

        var usuarioId = await _usuarioRepository.CrearAsync(nuevoUsuario);
        nuevoUsuario.Id = usuarioId;

        // Actualizar último acceso
        await _usuarioRepository.ActualizarUltimoAccesoAsync(usuarioId);

        // Generar token JWT
        var token = GenerateJwtToken(nuevoUsuario);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            Usuario = new UsuarioInfo
            {
                Id = nuevoUsuario.Id,
                Nombre = nuevoUsuario.Nombre,
                Apellido = nuevoUsuario.Apellido,
                Correo = nuevoUsuario.Correo,
                FotoPerfilUrl = nuevoUsuario.FotoPerfilUrl
            }
        };
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        // Verificar si el token está en la blacklist (Redis)
        var isBlacklisted = await _cacheService.GetAsync<string>($"blacklist:token:{token}");
        if (isBlacklisted != null)
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task InvalidateTokenAsync(string token)
    {
        // Agregar token a blacklist en Redis (hasta que expire)
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes);
        var timeToLive = expiresAt - DateTime.UtcNow;
        
        await _cacheService.SetAsync($"blacklist:token:{token}", token, timeToLive);
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSecret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim("UsuarioId", usuario.Id.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
            Issuer = _jwtIssuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
