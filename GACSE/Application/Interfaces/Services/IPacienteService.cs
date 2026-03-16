using GACSE.Application.DTOs;

namespace GACSE.Application.Interfaces.Services
{
    public interface IPacienteService
    {
        Task<List<PacienteResponseDTO>> ObtenerTodosAsync();
        Task<PacienteResponseDTO> ObtenerPorIdAsync(int id);
        Task<PacienteResponseDTO> CrearAsync(CrearPacienteDTO dto);
        Task<PacienteResponseDTO> ActualizarAsync(int id, ActualizarPacienteDTO dto);
        Task EliminarAsync(int id);
        Task<List<CitaResponseDTO>> ObtenerHistorialAsync(int pacienteId);
    }
}
