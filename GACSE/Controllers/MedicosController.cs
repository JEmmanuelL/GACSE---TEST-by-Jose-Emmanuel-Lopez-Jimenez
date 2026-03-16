using GACSE.Application.DTOs;
using GACSE.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GACSE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicosController : ControllerBase
    {
        private readonly IMedicoService _medicoService;

        public MedicosController(IMedicoService medicoService)
        {
            _medicoService = medicoService;
        }

        /// <summary>
        /// Obtiene la lista de todos los médicos.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<MedicoResponseDTO>>> ObtenerTodos()
        {
            var medicos = await _medicoService.ObtenerTodosAsync();
            return Ok(medicos);
        }

        /// <summary>
        /// Obtiene un médico por su Id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicoResponseDTO>> ObtenerPorId(int id)
        {
            var medico = await _medicoService.ObtenerPorIdAsync(id);
            return Ok(medico);
        }

        /// <summary>
        /// Crea un nuevo médico.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MedicoResponseDTO>> Crear([FromBody] CrearMedicoDTO dto)
        {
            var medico = await _medicoService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = medico.Id }, medico);
        }

        /// <summary>
        /// Actualiza un médico existente.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<MedicoResponseDTO>> Actualizar(int id, [FromBody] ActualizarMedicoDTO dto)
        {
            var medico = await _medicoService.ActualizarAsync(id, dto);
            return Ok(medico);
        }

        /// <summary>
        /// Elimina un médico.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _medicoService.EliminarAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Obtiene los próximos 5 horarios disponibles de un médico a partir de una fecha.
        /// </summary>
        [HttpGet("{id}/horarios-disponibles")]
        public async Task<ActionResult<List<HorarioDisponibleDTO>>> ObtenerHorariosDisponibles(int id, [FromQuery] DateTime fecha)
        {
            var horarios = await _medicoService.ObtenerHorariosDisponiblesAsync(id, fecha);
            return Ok(horarios);
        }
    }
}
