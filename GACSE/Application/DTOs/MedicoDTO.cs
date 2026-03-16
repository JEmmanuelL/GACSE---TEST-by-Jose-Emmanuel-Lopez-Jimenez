using GACSE.Domain.Enums;

namespace GACSE.Application.DTOs
{
    // ── Request DTOs ──

    public class CrearMedicoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public EspecialidadMedica Especialidad { get; set; }
        public List<HorarioMedicoDTO> Horarios { get; set; } = new();
    }

    public class ActualizarMedicoDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public EspecialidadMedica Especialidad { get; set; }
        public List<HorarioMedicoDTO> Horarios { get; set; } = new();
    }

    public class HorarioMedicoDTO
    {
        public DayOfWeek DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }

    // ── Response DTOs ──

    public class MedicoResponseDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public List<HorarioMedicoDTO> Horarios { get; set; } = new();
    }

    public class HorarioDisponibleDTO
    {
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string DiaSemana { get; set; } = string.Empty;
    }
}
