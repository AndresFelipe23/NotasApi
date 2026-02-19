-- =========================================
-- STORED PROCEDURES: IntegracionesGoogle
-- =========================================
USE Anota;
GO

-- ============================================================================
-- 1. CREAR O ACTUALIZAR INTEGRACIÓN DE GOOGLE
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_IntegracionesGoogle_Upsert
    @UsuarioId UNIQUEIDENTIFIER,
    @AccessToken NVARCHAR(MAX),
    @RefreshToken NVARCHAR(MAX),
    @TokenExpiresAt DATETIMEOFFSET,
    @GoogleUserId NVARCHAR(255) = NULL,
    @GoogleEmail NVARCHAR(255) = NULL,
    @GoogleName NVARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM IntegracionesGoogle WHERE UsuarioId = @UsuarioId)
    BEGIN
        -- Actualizar integración existente
        UPDATE IntegracionesGoogle
        SET 
            AccessToken = @AccessToken,
            RefreshToken = @RefreshToken,
            TokenExpiresAt = @TokenExpiresAt,
            GoogleUserId = ISNULL(@GoogleUserId, GoogleUserId),
            GoogleEmail = ISNULL(@GoogleEmail, GoogleEmail),
            GoogleName = ISNULL(@GoogleName, GoogleName),
            EstaActiva = 1,
            FechaActualizacion = GETUTCDATE()
        WHERE UsuarioId = @UsuarioId;
    END
    ELSE
    BEGIN
        -- Crear nueva integración
        INSERT INTO IntegracionesGoogle (
            UsuarioId, AccessToken, RefreshToken, TokenExpiresAt,
            GoogleUserId, GoogleEmail, GoogleName, EstaActiva
        )
        VALUES (
            @UsuarioId, @AccessToken, @RefreshToken, @TokenExpiresAt,
            @GoogleUserId, @GoogleEmail, @GoogleName, 1
        );
    END
END;
GO

-- ============================================================================
-- 2. OBTENER INTEGRACIÓN POR USUARIO
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_IntegracionesGoogle_ObtenerPorUsuario
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        UsuarioId,
        AccessToken,
        RefreshToken,
        TokenExpiresAt,
        GoogleUserId,
        GoogleEmail,
        GoogleName,
        EstaActiva,
        FechaConectada,
        FechaActualizacion
    FROM IntegracionesGoogle
    WHERE UsuarioId = @UsuarioId AND EstaActiva = 1;
END;
GO

-- ============================================================================
-- 3. ACTUALIZAR TOKENS (para refresh automático)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_IntegracionesGoogle_ActualizarTokens
    @UsuarioId UNIQUEIDENTIFIER,
    @AccessToken NVARCHAR(MAX),
    @TokenExpiresAt DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE IntegracionesGoogle
    SET 
        AccessToken = @AccessToken,
        TokenExpiresAt = @TokenExpiresAt,
        FechaActualizacion = GETUTCDATE()
    WHERE UsuarioId = @UsuarioId AND EstaActiva = 1;
END;
GO

-- ============================================================================
-- 4. DESCONECTAR INTEGRACIÓN (marcar como inactiva)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_IntegracionesGoogle_Desconectar
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE IntegracionesGoogle
    SET 
        EstaActiva = 0,
        FechaActualizacion = GETUTCDATE()
    WHERE UsuarioId = @UsuarioId;
    
    -- Opcional: Eliminar también las vinculaciones de tareas
    -- DELETE FROM TareasGoogle WHERE UsuarioId = @UsuarioId;
END;
GO
