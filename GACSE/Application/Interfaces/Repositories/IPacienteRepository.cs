using GACSE.Domain.Entities;

namespace GACSE.Application.Interfaces.Repositories
{
    public interface IPacienteRepository
    {
        Task<List<Paciente>> ObtenerTodosAsync();
        Task<Paciente?> ObtenerPorIdAsync(int id);
        Task<Paciente> CrearAsync(Paciente paciente);
        Task ActualizarAsync(Paciente paciente);
        Task EliminarAsync(Paciente paciente);
    }
}
