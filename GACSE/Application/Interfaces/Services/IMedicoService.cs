using GACSE.Application.DTOs;

namespace GACSE.Application.Interfaces.Services
{
    public interface IMedicoService
    {
        Task<List<MedicoResponseDTO>> ObtenerTodosAsync();
        Task<MedicoResponseDTO> ObtenerPorIdAsync(int id);
        Task<MedicoResponseDTO> CrearAsync(CrearMedicoDTO dto);
        Task<MedicoResponseDTO> ActualizarAsync(int id, ActualizarMedicoDTO dto);
        Task EliminarAsync(int id);
        Task<List<HorarioDisponibleDTO>> ObtenerHorariosDisponiblesAsync(int medicoId, DateTime fecha);
    }
}
