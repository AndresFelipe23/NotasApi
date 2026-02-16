using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotasApi.Data;
using NotasApi.Repositories;
using NotasApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger con soporte JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Anota API", Version = "v1" });
    
    // Agregar soporte JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurar JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AnotaAPI";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configurar Redis Cache
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "Anota:"; // Prefijo para todas las claves
    });
}
else
{
    // Fallback a MemoryCache si Redis no está configurado (útil para desarrollo)
    builder.Services.AddDistributedMemoryCache();
}

// Registrar CacheService
builder.Services.AddScoped<ICacheService, CacheService>();

// Registrar AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

// Registrar DbConnectionFactory
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();

// Registrar Repositorios Base (sin caché)
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<CarpetaRepository>(); // Implementación base
builder.Services.AddScoped<NotaRepository>(); // Implementación base
builder.Services.AddScoped<NotaRapidaRepository>(); // Implementación base
builder.Services.AddScoped<TareaRepository>(); // Implementación base

// Registrar Repositorios con Caché (Decoradores)
// Solo los que necesitan caché según la estrategia
builder.Services.AddScoped<ICarpetaRepository>(sp =>
{
    var baseRepo = sp.GetRequiredService<CarpetaRepository>();
    var cache = sp.GetRequiredService<ICacheService>();
    return new CarpetaRepositoryCacheDecorator(baseRepo, cache);
});

builder.Services.AddScoped<INotaRepository, NotaRepository>(); // Sin caché (las notas se leen individualmente)

// Notas rápidas sin caché para máxima velocidad (se puede habilitar Redis más adelante)
builder.Services.AddScoped<INotaRapidaRepository, NotaRapidaRepository>();

builder.Services.AddScoped<ITareaRepository>(sp =>
{
    var baseRepo = sp.GetRequiredService<TareaRepository>();
    var cache = sp.GetRequiredService<ICacheService>();
    return new TareaRepositoryCacheDecorator(baseRepo, cache);
});

// Configuracion de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Y en el pipeline:
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
