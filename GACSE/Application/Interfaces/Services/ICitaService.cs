using GACSE.Application.DTOs;

namespace GACSE.Application.Interfaces.Services
{
    public interface ICitaService
    {
        Task<AgendarCitaResponseDTO> AgendarCitaAsync(AgendarCitaDTO dto);
        Task<CitaResponseDTO> CancelarCitaAsync(int id);
        Task<List<CitaResponseDTO>> ConsultarCitasAsync();
        Task<AgendaDiaDTO> ObtenerAgendaDelDiaAsync(int medicoId, DateTime fecha);
    }
}
