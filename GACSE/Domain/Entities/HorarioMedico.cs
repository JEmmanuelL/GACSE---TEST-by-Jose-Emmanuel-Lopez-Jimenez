namespace GACSE.Domain.Entities
{
    public class HorarioMedico
    {
        public int Id { get; set; }
        public int MedicoId { get; set; }
        public DayOfWeek DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        // Navegación
        public Medico Medico { get; set; } = null!;
    }
}
