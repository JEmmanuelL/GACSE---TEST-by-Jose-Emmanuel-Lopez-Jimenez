using GACSE.Domain.Enums;

namespace GACSE.Domain.Entities
{
    public class Medico
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public EspecialidadMedica Especialidad { get; set; }

        // Navegación
        public ICollection<HorarioMedico> Horarios { get; set; } = new List<HorarioMedico>();
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
