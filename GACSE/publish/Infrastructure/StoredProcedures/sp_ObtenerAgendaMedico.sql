-- =============================================
-- Stored Procedure: sp_ObtenerAgendaMedico
-- Descripción: Obtiene la agenda completa del día de un médico
-- incluyendo datos del paciente y estado de la cita.
-- =============================================
CREATE OR ALTER PROCEDURE sp_ObtenerAgendaMedico
    @MedicoId INT,
    @Fecha DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        c.Id,
        c.MedicoId,
        m.Nombre AS NombreMedico,
        m.Especialidad,
        c.PacienteId,
        p.Nombre AS NombrePaciente,
        c.Fecha,
        c.Hora,
        c.Motivo,
        c.Estado,
        CASE 
            WHEN m.Especialidad = 'MedicinaGeneral' THEN 20
            WHEN m.Especialidad = 'Cardiologia' THEN 30
            WHEN m.Especialidad = 'Cirugia' THEN 45
            WHEN m.Especialidad = 'Pediatria' THEN 20
            WHEN m.Especialidad = 'Ginecologia' THEN 30
            ELSE 30
        END AS DuracionMinutos
    FROM Citas c
    INNER JOIN Medicos m ON m.Id = c.MedicoId
    INNER JOIN Pacientes p ON p.Id = c.PacienteId
    WHERE c.MedicoId = @MedicoId
      AND c.Fecha = @Fecha
    ORDER BY c.Hora ASC;
END
