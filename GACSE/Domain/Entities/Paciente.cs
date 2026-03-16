namespace GACSE.Domain.Entities
{
    public class Paciente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;

        // Navegación
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
