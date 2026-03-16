using GACSE.Domain.Enums;

namespace GACSE.Domain.Constants
{
    public static class DuracionCitas
    {
        public static readonly Dictionary<EspecialidadMedica, int> MinutosPorEspecialidad = new()
        {
            { EspecialidadMedica.MedicinaGeneral, 20 },
            { EspecialidadMedica.Cardiologia, 30 },
            { EspecialidadMedica.Cirugia, 45 },
            { EspecialidadMedica.Pediatria, 20 },
            { EspecialidadMedica.Ginecologia, 30 }
        };

        public static int ObtenerDuracion(EspecialidadMedica especialidad)
        {
            return MinutosPorEspecialidad.TryGetValue(especialidad, out var minutos)
                ? minutos
                : 30; // duración por defecto
        }
    }
}
