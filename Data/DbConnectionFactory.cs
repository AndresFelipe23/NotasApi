using System.Data;
using Microsoft.Data.SqlClient;

namespace NotasApi.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(IConfiguration configuration)
    {
        var baseConnectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        
        // Optimizaciones para velocidad:
        // - Connection Pooling activado por defecto (máximo rendimiento)
        // - Command Timeout: 30 segundos (suficiente para operaciones rápidas)
        // - MARS (Multiple Active Result Sets): desactivado para mejor rendimiento
        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            Pooling = true, // Connection pooling activado (por defecto, pero explícito)
            MaxPoolSize = 100, // Máximo de conexiones en el pool
            MinPoolSize = 5, // Mínimo de conexiones siempre listas
            ConnectTimeout = 15, // Timeout de conexión: 15 segundos
            CommandTimeout = 30, // Timeout de comandos: 30 segundos
            MultipleActiveResultSets = false // MARS desactivado para mejor rendimiento
        };
        
        _connectionString = builder.ConnectionString;
    }

    public IDbConnection CreateConnection()
    {
        // SqlConnection automáticamente usa connection pooling
        // Esto es MUY rápido porque reutiliza conexiones existentes
        return new SqlConnection(_connectionString);
    }
}
