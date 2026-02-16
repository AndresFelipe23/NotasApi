USE Anota;
GO

-- Script para verificar y crear/actualizar el stored procedure de Notas Rápidas
-- Ejecuta este script en SQL Server Management Studio

-- Verificar si el stored procedure existe
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_NotasRapidas_ObtenerTodas]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'El stored procedure usp_NotasRapidas_ObtenerTodas existe.';
    -- Mostrar la definición actual
    EXEC sp_helptext 'usp_NotasRapidas_ObtenerTodas';
END
ELSE
BEGIN
    PRINT 'El stored procedure usp_NotasRapidas_ObtenerTodas NO existe.';
    PRINT 'Ejecuta el archivo "SP DE NOTAS RAPIDAS.sql" para crearlo.';
END
GO

-- Verificar la estructura de la tabla
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[NotasRapidas]') AND type in (N'U'))
BEGIN
    PRINT 'La tabla NotasRapidas existe.';
    -- Mostrar la estructura de la tabla
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE,
        CHARACTER_MAXIMUM_LENGTH
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'NotasRapidas'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'ERROR: La tabla NotasRapidas NO existe.';
    PRINT 'Necesitas crear la tabla primero.';
END
GO

-- Probar el stored procedure (si existe y hay datos)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_NotasRapidas_ObtenerTodas]') AND type in (N'P', N'PC'))
BEGIN
    PRINT 'Probando el stored procedure con un UsuarioId de prueba...';
    DECLARE @TestUsuarioId UNIQUEIDENTIFIER;
    
    -- Obtener el primer usuario de la tabla Usuarios
    SELECT TOP 1 @TestUsuarioId = Id FROM Usuarios;
    
    IF @TestUsuarioId IS NOT NULL
    BEGIN
        PRINT 'UsuarioId de prueba: ' + CAST(@TestUsuarioId AS VARCHAR(36));
        EXEC usp_NotasRapidas_ObtenerTodas @UsuarioId = @TestUsuarioId;
    END
    ELSE
    BEGIN
        PRINT 'No hay usuarios en la tabla Usuarios para probar.';
    END
END
GO
