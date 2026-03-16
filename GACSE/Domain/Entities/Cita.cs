using GACSE.Domain.Enums;

namespace GACSE.Domain.Entities
{
    public class Cita
    {
        public int Id { get; set; }
        public int MedicoId { get; set; }
        public int PacienteId { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public EstadoCita Estado { get; set; } = EstadoCita.Programada;

        // Navegación
        public Medico Medico { get; set; } = null!;
        public Paciente Paciente { get; set; } = null!;
    }
}
