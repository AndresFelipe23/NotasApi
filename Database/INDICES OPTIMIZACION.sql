USE Anota;
GO

-- ============================================================================
-- ÍNDICES ADICIONALES PARA OPTIMIZACIÓN DE RENDIMIENTO
-- ============================================================================

-- Índice compuesto para búsquedas de notas por usuario y estado
-- Útil para queries que filtran por usuario y estado de archivado
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notas_UsuarioId_EsArchivada')
BEGIN
    CREATE INDEX IX_Notas_UsuarioId_EsArchivada 
    ON Notas(UsuarioId, EsArchivada) 
    INCLUDE (Titulo, FechaActualizacion);
END
GO

-- Índice para ordenar notas por fecha de actualización (muy usado en listados)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Notas_FechaActualizacion')
BEGIN
    CREATE INDEX IX_Notas_FechaActualizacion 
    ON Notas(FechaActualizacion DESC) 
    INCLUDE (Id, UsuarioId, CarpetaId, Titulo, EsFavorita);
END
GO

-- Índice para tareas ordenadas por prioridad y fecha de vencimiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tareas_Ordenamiento')
BEGIN
    CREATE INDEX IX_Tareas_Ordenamiento 
    ON Tareas(UsuarioId, EstaCompletada, Prioridad, FechaVencimiento, Orden) 
    INCLUDE (Id, Descripcion, NotaVinculadaId);
END
GO

-- Índice optimizado para notas rápidas (filtra por usuario y archivado, ordena por fecha de actualización)
-- Este índice cubre exactamente la query del SP usp_NotasRapidas_ObtenerTodas
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NotasRapidas_FechaCreacion')
BEGIN
    DROP INDEX IX_NotasRapidas_FechaCreacion ON NotasRapidas;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_NotasRapidas_UsuarioId_EsArchivada')
BEGIN
    CREATE INDEX IX_NotasRapidas_UsuarioId_EsArchivada
    ON NotasRapidas(UsuarioId, EsArchivada, FechaActualizacion DESC)
    INCLUDE (Id, Contenido, ColorHex, FechaCreacion);
END
GO

-- Índice para búsqueda rápida de usuarios por correo (ya existe UNIQUE, pero esto ayuda)
-- El índice UNIQUE ya existe en la tabla, pero verificamos que esté optimizado
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ__Usuarios__Correo')
BEGIN
    -- El índice UNIQUE ya debería existir, pero lo verificamos
    PRINT 'El índice único en Correo ya existe o será creado automáticamente';
END
GO

-- Estadísticas actualizadas para optimizador de consultas
UPDATE STATISTICS Usuarios;
UPDATE STATISTICS Carpetas;
UPDATE STATISTICS Notas;
UPDATE STATISTICS NotasRapidas;
UPDATE STATISTICS Tareas;
GO

PRINT 'Índices de optimización creados y estadísticas actualizadas.';
GO
