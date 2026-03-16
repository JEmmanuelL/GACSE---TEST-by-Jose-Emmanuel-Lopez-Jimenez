using GACSE.Application.DTOs;
using GACSE.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GACSE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitasController : ControllerBase
    {
        private readonly ICitaService _citaService;

        public CitasController(ICitaService citaService)
        {
            _citaService = citaService;
        }

        /// <summary>
        /// Agenda una nueva cita médica.
        /// </summary>
        /// <remarks>
        /// Reglas de negocio:
        /// - No se permiten citas en el pasado
        /// - La cita debe estar dentro del horario del médico
        /// - No puede haber conflictos de horario (citas simultáneas)
        /// - Si hay conflicto, se sugieren los próximos 5 horarios disponibles
        /// - Si el paciente tiene 3+ cancelaciones en los últimos 30 días, se incluye una alerta
        /// </remarks>
        [HttpPost]
        public async Task<ActionResult<AgendarCitaResponseDTO>> AgendarCita([FromBody] AgendarCitaDTO dto)
        {
            var resultado = await _citaService.AgendarCitaAsync(dto);
            return CreatedAtAction(nameof(ConsultarCitas), null, resultado);
        }

        /// <summary>
        /// Cancela una cita existente.
        /// </summary>
        [HttpPut("{id}/cancelar")]
        public async Task<ActionResult<CitaResponseDTO>> CancelarCita(int id)
        {
            var cita = await _citaService.CancelarCitaAsync(id);
            return Ok(cita);
        }

        /// <summary>
        /// Consulta todas las citas registradas.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<CitaResponseDTO>>> ConsultarCitas()
        {
            var citas = await _citaService.ConsultarCitasAsync();
            return Ok(citas);
        }

        /// <summary>
        /// Obtiene la agenda completa de un médico para una fecha específica, ordenada por hora.
        /// </summary>
        [HttpGet("agenda")]
        public async Task<ActionResult<AgendaDiaDTO>> ObtenerAgendaDelDia([FromQuery] int medicoId, [FromQuery] DateTime fecha)
        {
            var agenda = await _citaService.ObtenerAgendaDelDiaAsync(medicoId, fecha);
            return Ok(agenda);
        }
    }
}
