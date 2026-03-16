using GACSE.Domain.Entities;

namespace GACSE.Application.Interfaces.Repositories
{
    public interface ICitaRepository
    {
        Task<List<Cita>> ObtenerTodosAsync();
        Task<Cita?> ObtenerPorIdAsync(int id);
        Task<Cita> CrearAsync(Cita cita);
        Task ActualizarAsync(Cita cita);

        /// <summary>
        /// Obtiene citas de un médico en una fecha específica, ordenadas por hora.
        /// </summary>
        Task<List<Cita>> ObtenerCitasPorMedicoYFechaAsync(int medicoId, DateTime fecha);

        /// <summary>
        /// Obtiene todas las citas de un paciente (historial).
        /// </summary>
        Task<List<Cita>> ObtenerCitasPorPacienteAsync(int pacienteId);

        /// <summary>
        /// Verifica si existe conflicto de horario para un médico en una fecha/hora dada.
        /// Usa el stored procedure sp_ValidarDisponibilidadMedico.
        /// </summary>
        Task<bool> ValidarDisponibilidadMedicoAsync(int medicoId, DateTime fecha, TimeSpan hora, int duracionMinutos);

        /// <summary>
        /// Obtiene la agenda completa del día de un médico usando stored procedure.
        /// </summary>
        Task<List<Cita>> ObtenerAgendaMedicoSpAsync(int medicoId, DateTime fecha);

        /// <summary>
        /// Cuenta las cancelaciones de un paciente en los últimos N días.
        /// </summary>
        Task<int> ContarCancelacionesRecientesAsync(int pacienteId, int dias);
    }
}
