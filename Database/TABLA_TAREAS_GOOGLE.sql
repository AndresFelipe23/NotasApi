-- =========================================
-- TABLA: TareasGoogle
-- Propósito: Mapear tareas de Anota con tareas de Google Tasks para sincronización bidireccional
-- =========================================
USE Anota;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TareasGoogle')
BEGIN
    CREATE TABLE TareasGoogle (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TareaId UNIQUEIDENTIFIER NOT NULL,
        UsuarioId UNIQUEIDENTIFIER NOT NULL,
        
        -- IDs de Google Tasks
        GoogleTaskListId NVARCHAR(255) NOT NULL,
        GoogleTaskId NVARCHAR(255) NOT NULL,
        
        -- Metadatos de sincronización
        UltimaSincronizacion DATETIMEOFFSET DEFAULT GETUTCDATE(),
        SincronizarDesdeGoogle BIT DEFAULT 1, -- Si cambios en Google deben actualizar Anota
        SincronizarHaciaGoogle BIT DEFAULT 1, -- Si cambios en Anota deben actualizar Google
        
        -- Timestamps para resolución de conflictos
        UltimaModificacionEnGoogle DATETIMEOFFSET NULL,
        UltimaModificacionEnAnota DATETIMEOFFSET NULL,
        
        CONSTRAINT FK_TareasGoogle_Tareas FOREIGN KEY (TareaId) REFERENCES Tareas(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TareasGoogle_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_TareasGoogle_TareaId UNIQUE (TareaId), -- Una tarea de Anota solo puede estar vinculada a una tarea de Google
        CONSTRAINT UQ_TareasGoogle_GoogleTask UNIQUE (UsuarioId, GoogleTaskListId, GoogleTaskId) -- Una tarea de Google solo puede estar vinculada a una tarea de Anota
    );
    
    CREATE INDEX IX_TareasGoogle_TareaId ON TareasGoogle(TareaId);
    CREATE INDEX IX_TareasGoogle_UsuarioId ON TareasGoogle(UsuarioId);
    CREATE INDEX IX_TareasGoogle_GoogleTask ON TareasGoogle(UsuarioId, GoogleTaskListId, GoogleTaskId);
    -- Índice filtrado: SQL Server no permite OR en índices filtrados, usamos índice compuesto normal
    CREATE INDEX IX_TareasGoogle_UsuarioId_Sincronizar ON TareasGoogle(UsuarioId, SincronizarDesdeGoogle, SincronizarHaciaGoogle);
    
    PRINT 'Tabla TareasGoogle creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla TareasGoogle ya existe.';
END
GO
