-- =========================================
-- TABLA: IntegracionesGoogle
-- Propósito: Guardar tokens OAuth de Google Tasks por usuario
-- =========================================
USE Anota;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IntegracionesGoogle')
BEGIN
    CREATE TABLE IntegracionesGoogle (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UsuarioId UNIQUEIDENTIFIER NOT NULL,
        
        -- Tokens OAuth
        AccessToken NVARCHAR(MAX) NOT NULL,
        RefreshToken NVARCHAR(MAX) NOT NULL,
        TokenExpiresAt DATETIMEOFFSET NOT NULL,
        
        -- Información de Google
        GoogleUserId NVARCHAR(255) NULL,
        GoogleEmail NVARCHAR(255) NULL,
        GoogleName NVARCHAR(255) NULL,
        
        -- Estado
        EstaActiva BIT DEFAULT 1,
        FechaConectada DATETIMEOFFSET DEFAULT GETUTCDATE(),
        FechaActualizacion DATETIMEOFFSET DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_IntegracionesGoogle_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_IntegracionesGoogle_UsuarioId UNIQUE (UsuarioId)
    );
    
    CREATE INDEX IX_IntegracionesGoogle_UsuarioId ON IntegracionesGoogle(UsuarioId);
    CREATE INDEX IX_IntegracionesGoogle_EstaActiva ON IntegracionesGoogle(UsuarioId) WHERE EstaActiva = 1;
    
    PRINT 'Tabla IntegracionesGoogle creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla IntegracionesGoogle ya existe.';
END
GO
