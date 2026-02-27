-- =========================================
-- CREACIÓN DE LA BASE DE DATOS
-- =========================================
CREATE DATABASE Anota;
GO

USE Anota;
GO

-- =========================================
-- 1. TABLA DE USUARIOS
-- =========================================
CREATE TABLE Usuarios (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NULL,
    Correo NVARCHAR(150) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL, 
    FotoPerfilUrl NVARCHAR(500) NULL,
    EsActivo BIT DEFAULT 1, 
    FechaUltimoAcceso DATETIME2 NULL,
    FechaCreacion DATETIME2 DEFAULT GETUTCDATE(),
    FechaActualizacion DATETIME2 DEFAULT GETUTCDATE()
);
GO

-- =========================================
-- 2. TABLA DE CARPETAS
-- =========================================
CREATE TABLE Carpetas (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    RutaJerarquica HIERARCHYID NOT NULL, 
    Nivel AS RutaJerarquica.GetLevel() PERSISTED, 
    Icono NVARCHAR(50) NULL, 
    ColorHex VARCHAR(7) NULL, 
    Orden INT DEFAULT 0, 
    FechaCreacion DATETIME2 DEFAULT GETUTCDATE(),
    FechaActualizacion DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Carpetas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
GO

CREATE UNIQUE INDEX IX_Carpetas_RutaJerarquica ON Carpetas(RutaJerarquica);
CREATE INDEX IX_Carpetas_UsuarioId ON Carpetas(UsuarioId);
GO

-- =========================================
-- 3. TABLA DE NOTAS
-- =========================================
CREATE TABLE Notas (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    CarpetaId UNIQUEIDENTIFIER NULL, 
    Titulo NVARCHAR(200) NOT NULL,
    Resumen NVARCHAR(300) NULL, 
    Icono NVARCHAR(50) NULL,
    ColorHex VARCHAR(7) NULL,
    ImagenPortadaUrl NVARCHAR(500) NULL, 
    ContenidoBloques NVARCHAR(MAX) NULL, 
    EsFavorita BIT DEFAULT 0,
    EsArchivada BIT DEFAULT 0,
    EsPublica BIT DEFAULT 0, 
    FechaCreacion DATETIME2 DEFAULT GETUTCDATE(),
    FechaActualizacion DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Notas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_Notas_Carpetas FOREIGN KEY (CarpetaId) REFERENCES Carpetas(Id),
    CONSTRAINT CK_Notas_ContenidoEsJson CHECK (ContenidoBloques IS NULL OR ISJSON(ContenidoBloques) = 1)
);
GO

CREATE INDEX IX_Notas_CarpetaId ON Notas(CarpetaId);
CREATE INDEX IX_Notas_Favoritas ON Notas(UsuarioId) WHERE EsFavorita = 1;
GO

-- =========================================
-- 4. TABLA DE NOTAS RÁPIDAS
-- =========================================
CREATE TABLE NotasRapidas (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    Contenido NVARCHAR(MAX) NOT NULL, 
    ColorHex VARCHAR(7) NULL, 
    EsArchivada BIT DEFAULT 0, 
    FechaCreacion DATETIME2 DEFAULT DATEADD(HOUR, -5, GETUTCDATE()),
    FechaActualizacion DATETIME2 DEFAULT DATEADD(HOUR, -5, GETUTCDATE()),

    CONSTRAINT FK_NotasRapidas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
GO

CREATE INDEX IX_NotasRapidas_Activas ON NotasRapidas(UsuarioId) WHERE EsArchivada = 0;
GO

-- =========================================
-- 5. TABLA DE TAREAS
-- =========================================
CREATE TABLE Tareas (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    NotaVinculadaId UNIQUEIDENTIFIER NULL, 
    Descripcion NVARCHAR(500) NOT NULL,
    EstaCompletada BIT DEFAULT 0,
    Prioridad INT DEFAULT 2, 
    Orden INT DEFAULT 0, 
    FechaVencimiento DATETIMEOFFSET NULL, 
    FechaCreacion DATETIME2 DEFAULT GETUTCDATE(),
    FechaCompletada DATETIME2 NULL,

    CONSTRAINT FK_Tareas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id),
    CONSTRAINT FK_Tareas_Notas FOREIGN KEY (NotaVinculadaId) REFERENCES Notas(Id)
);
GO

CREATE INDEX IX_Tareas_Pendientes ON Tareas(UsuarioId) WHERE EstaCompletada = 0;
GO

-- =========================================
-- 6. TABLA DE ETIQUETAS
-- =========================================
CREATE TABLE Etiquetas (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    Nombre NVARCHAR(50) NOT NULL,
    ColorHex VARCHAR(7) NULL, 
    FechaCreacion DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Etiquetas_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
GO

-- =========================================
-- 7. TABLA INTERMEDIA (Notas - Etiquetas)
-- =========================================
CREATE TABLE NotasEtiquetas (
    NotaId UNIQUEIDENTIFIER NOT NULL,
    EtiquetaId UNIQUEIDENTIFIER NOT NULL,
    
    PRIMARY KEY (NotaId, EtiquetaId),
    
    CONSTRAINT FK_NotasEtiquetas_Notas FOREIGN KEY (NotaId) REFERENCES Notas(Id) ON DELETE CASCADE,
    CONSTRAINT FK_NotasEtiquetas_Etiquetas FOREIGN KEY (EtiquetaId) REFERENCES Etiquetas(Id) ON DELETE CASCADE
);
GO