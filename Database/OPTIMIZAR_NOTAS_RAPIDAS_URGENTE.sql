USE Anota;
GO

-- ============================================================================
-- SCRIPT DE OPTIMIZACIÓN URGENTE PARA NOTAS RÁPIDAS
-- Ejecuta este script para mejorar el rendimiento inmediatamente
-- ============================================================================

PRINT '=== INICIANDO OPTIMIZACIÓN DE NOTAS RÁPIDAS ===';
GO

-- 1. Eliminar índice antiguo si existe (ordenaba por FechaCreacion en lugar de FechaActualizacion)
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NotasRapidas_FechaCreacion' AND object_id = OBJECT_ID('NotasRapidas'))
BEGIN
    PRINT 'Eliminando índice antiguo IX_NotasRapidas_FechaCreacion...';
    DROP INDEX IX_NotasRapidas_FechaCreacion ON NotasRapidas;
    PRINT 'Índice antiguo eliminado.';
END
ELSE
BEGIN
    PRINT 'Índice antiguo no encontrado (OK).';
END
GO

-- 2. Crear nuevo índice optimizado (ordena por FechaActualizacion como lo requiere el SP)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NotasRapidas_UsuarioId_EsArchivada' AND object_id = OBJECT_ID('NotasRapidas'))
BEGIN
    PRINT 'Creando índice optimizado IX_NotasRapidas_UsuarioId_EsArchivada...';
    CREATE INDEX IX_NotasRapidas_UsuarioId_EsArchivada
    ON NotasRapidas(UsuarioId, EsArchivada, FechaActualizacion DESC)
    INCLUDE (Id, Contenido, ColorHex, FechaCreacion);
    PRINT 'Índice optimizado creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'Índice optimizado ya existe (OK).';
END
GO

-- 3. Actualizar estadísticas para el optimizador de consultas
PRINT 'Actualizando estadísticas de NotasRapidas...';
UPDATE STATISTICS NotasRapidas WITH FULLSCAN;
PRINT 'Estadísticas actualizadas.';
GO

-- 4. Verificar índices existentes
PRINT '';
PRINT '=== ÍNDICES ACTUALES EN TABLA NotasRapidas ===';
SELECT
    i.name AS IndiceNombre,
    i.type_desc AS TipoIndice,
    COL_NAME(ic.object_id, ic.column_id) AS Columna,
    ic.key_ordinal AS OrdenColumna,
    ic.is_descending_key AS Descendente,
    ic.is_included_column AS EsIncluida
FROM sys.indexes i
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('NotasRapidas')
ORDER BY i.name, ic.key_ordinal, ic.is_included_column;
GO

-- 5. Probar el rendimiento del SP
PRINT '';
PRINT '=== PROBANDO RENDIMIENTO DEL SP ===';
SET STATISTICS TIME ON;
SET STATISTICS IO ON;

-- Reemplaza este GUID con un UsuarioId real de tu base de datos para probar
DECLARE @TestUsuarioId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Usuarios);

IF @TestUsuarioId IS NOT NULL
BEGIN
    PRINT 'Ejecutando usp_NotasRapidas_ObtenerTodas para usuario: ' + CAST(@TestUsuarioId AS VARCHAR(36));
    EXEC usp_NotasRapidas_ObtenerTodas @UsuarioId = @TestUsuarioId;
END
ELSE
BEGIN
    PRINT 'No se encontró ningún usuario en la base de datos para probar.';
END

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
GO

PRINT '';
PRINT '=== OPTIMIZACIÓN COMPLETADA ===';
PRINT 'Si el tiempo de ejecución es > 100ms, verifica:';
PRINT '1. Que el índice IX_NotasRapidas_UsuarioId_EsArchivada exista';
PRINT '2. Que no haya bloqueos en la tabla';
PRINT '3. Que la conexión a SQL Server sea rápida';
GO
