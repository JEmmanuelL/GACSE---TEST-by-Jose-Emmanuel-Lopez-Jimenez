-- =============================================
-- Stored Procedure: sp_ValidarDisponibilidadMedico
-- Descripción: Valida si un médico tiene disponibilidad 
-- para una fecha y hora específicas considerando la duración de la cita.
-- Retorna 1 si está disponible, 0 si hay conflicto.
-- =============================================
CREATE OR ALTER PROCEDURE sp_ValidarDisponibilidadMedico
    @MedicoId INT,
    @Fecha DATE,
    @Hora TIME,
    @DuracionMinutos INT,
    @Disponible BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @HoraFin TIME = DATEADD(MINUTE, @DuracionMinutos, @Hora);

    -- Verificar si existe alguna cita que se solape con el rango solicitado
    IF EXISTS (
        SELECT 1
        FROM Citas c
        INNER JOIN Medicos m ON m.Id = c.MedicoId
        WHERE c.MedicoId = @MedicoId
          AND c.Fecha = @Fecha
          AND c.Estado != 'Cancelada'
          AND (
              -- La cita existente comienza durante el rango solicitado
              (c.Hora >= @Hora AND c.Hora < @HoraFin)
              OR
              -- La cita existente termina durante el rango solicitado
              (DATEADD(MINUTE, 
                  CASE 
                      WHEN m.Especialidad = 'MedicinaGeneral' THEN 20
                      WHEN m.Especialidad = 'Cardiologia' THEN 30
                      WHEN m.Especialidad = 'Cirugia' THEN 45
                      WHEN m.Especialidad = 'Pediatria' THEN 20
                      WHEN m.Especialidad = 'Ginecologia' THEN 30
                      ELSE 30
                  END, c.Hora) > @Hora AND c.Hora < @Hora)
          )
    )
    BEGIN
        SET @Disponible = 0;
    END
    ELSE
    BEGIN
        SET @Disponible = 1;
    END
END
