using GACSE.Domain.Enums;

namespace GACSE.Application.DTOs
{
    // ── Request DTOs ──

    public class AgendarCitaDTO
    {
        public int MedicoId { get; set; }
        public int PacienteId { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    // ── Response DTOs ──

    public class CitaResponseDTO
    {
        public int Id { get; set; }
        public int MedicoId { get; set; }
        public string NombreMedico { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public int PacienteId { get; set; }
        public string NombrePaciente { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int DuracionMinutos { get; set; }
    }

    public class AgendarCitaResponseDTO
    {
        public CitaResponseDTO Cita { get; set; } = null!;
        public bool AlertaCancelaciones { get; set; }
        public string? MensajeAlerta { get; set; }
    }

    public class ConflictoHorarioResponseDTO
    {
        public string Mensaje { get; set; } = string.Empty;
        public List<HorarioDisponibleDTO> HorariosDisponibles { get; set; } = new();
    }

    public class AgendaDiaDTO
    {
        public int MedicoId { get; set; }
        public string NombreMedico { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public List<CitaResponseDTO> Citas { get; set; } = new();
    }
}
