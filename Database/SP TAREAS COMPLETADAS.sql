-- Ejecuta este script en tu base de datos Anota si el endpoint /api/tareas/completadas devuelve 500
USE Anota;
GO

CREATE OR ALTER PROCEDURE usp_Tareas_ObtenerCompletadas
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, Descripcion, NotaVinculadaId, Prioridad, Orden, 
        EstaCompletada, FechaVencimiento,
        DATEADD(HOUR, -5, FechaCreacion) AS FechaCreacion,
        DATEADD(HOUR, -5, FechaCompletada) AS FechaCompletada
    FROM Tareas
    WHERE UsuarioId = @UsuarioId AND EstaCompletada = 1
    ORDER BY FechaCompletada DESC;
END;
GO
