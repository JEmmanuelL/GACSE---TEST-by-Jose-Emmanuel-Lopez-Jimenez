using GACSE.Domain.Entities;

namespace GACSE.Application.Interfaces.Repositories
{
    public interface IMedicoRepository
    {
        Task<List<Medico>> ObtenerTodosAsync();
        Task<Medico?> ObtenerPorIdAsync(int id);
        Task<Medico?> ObtenerPorIdConHorariosAsync(int id);
        Task<Medico> CrearAsync(Medico medico);
        Task ActualizarAsync(Medico medico);
        Task EliminarAsync(Medico medico);
    }
}
