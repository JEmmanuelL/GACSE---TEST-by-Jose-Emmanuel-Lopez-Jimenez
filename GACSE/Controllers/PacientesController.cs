using GACSE.Application.DTOs;
using GACSE.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace GACSE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;

        public PacientesController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService;
        }

        /// <summary>
        /// Obtiene la lista de todos los pacientes.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PacienteResponseDTO>>> ObtenerTodos()
        {
            var pacientes = await _pacienteService.ObtenerTodosAsync();
            return Ok(pacientes);
        }

        /// <summary>
        /// Obtiene un paciente por su Id.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PacienteResponseDTO>> ObtenerPorId(int id)
        {
            var paciente = await _pacienteService.ObtenerPorIdAsync(id);
            return Ok(paciente);
        }

        /// <summary>
        /// Crea un nuevo paciente.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PacienteResponseDTO>> Crear([FromBody] CrearPacienteDTO dto)
        {
            var paciente = await _pacienteService.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = paciente.Id }, paciente);
        }

        /// <summary>
        /// Actualiza un paciente existente.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<PacienteResponseDTO>> Actualizar(int id, [FromBody] ActualizarPacienteDTO dto)
        {
            var paciente = await _pacienteService.ActualizarAsync(id, dto);
            return Ok(paciente);
        }

        /// <summary>
        /// Elimina un paciente.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _pacienteService.EliminarAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Obtiene el historial completo de citas de un paciente (pasadas y futuras).
        /// </summary>
        [HttpGet("{id}/historial")]
        public async Task<ActionResult<List<CitaResponseDTO>>> ObtenerHistorial(int id)
        {
            var historial = await _pacienteService.ObtenerHistorialAsync(id);
            return Ok(historial);
        }
    }
}
