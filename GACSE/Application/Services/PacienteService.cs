using GACSE.Application.DTOs;
using GACSE.Application.Interfaces.Repositories;
using GACSE.Application.Interfaces.Services;
using GACSE.Domain.Constants;
using GACSE.Domain.Entities;

namespace GACSE.Application.Services
{
    public class PacienteService : IPacienteService
    {
        private readonly IPacienteRepository _pacienteRepository;
        private readonly ICitaRepository _citaRepository;

        public PacienteService(IPacienteRepository pacienteRepository, ICitaRepository citaRepository)
        {
            _pacienteRepository = pacienteRepository;
            _citaRepository = citaRepository;
        }

        public async Task<List<PacienteResponseDTO>> ObtenerTodosAsync()
        {
            var pacientes = await _pacienteRepository.ObtenerTodosAsync();
            return pacientes.Select(MapToResponse).ToList();
        }

        public async Task<PacienteResponseDTO> ObtenerPorIdAsync(int id)
        {
            var paciente = await _pacienteRepository.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró el paciente con Id {id}.");
            return MapToResponse(paciente);
        }

        public async Task<PacienteResponseDTO> CrearAsync(CrearPacienteDTO dto)
        {
            ValidarDatos(dto.Nombre, dto.CorreoElectronico);

            var paciente = new Paciente
            {
                Nombre = dto.Nombre,
                FechaNacimiento = dto.FechaNacimiento,
                Telefono = dto.Telefono,
                CorreoElectronico = dto.CorreoElectronico
            };

            await _pacienteRepository.CrearAsync(paciente);
            return MapToResponse(paciente);
        }

        public async Task<PacienteResponseDTO> ActualizarAsync(int id, ActualizarPacienteDTO dto)
        {
            var paciente = await _pacienteRepository.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró el paciente con Id {id}.");

            ValidarDatos(dto.Nombre, dto.CorreoElectronico);

            paciente.Nombre = dto.Nombre;
            paciente.FechaNacimiento = dto.FechaNacimiento;
            paciente.Telefono = dto.Telefono;
            paciente.CorreoElectronico = dto.CorreoElectronico;

            await _pacienteRepository.ActualizarAsync(paciente);
            return MapToResponse(paciente);
        }

        public async Task EliminarAsync(int id)
        {
            var paciente = await _pacienteRepository.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró el paciente con Id {id}.");
            await _pacienteRepository.EliminarAsync(paciente);
        }

        public async Task<List<CitaResponseDTO>> ObtenerHistorialAsync(int pacienteId)
        {
            // Verificar que el paciente existe
            _ = await _pacienteRepository.ObtenerPorIdAsync(pacienteId)
                ?? throw new KeyNotFoundException($"No se encontró el paciente con Id {pacienteId}.");

            var citas = await _citaRepository.ObtenerCitasPorPacienteAsync(pacienteId);

            return citas.Select(c => new CitaResponseDTO
            {
                Id = c.Id,
                MedicoId = c.MedicoId,
                NombreMedico = c.Medico.Nombre,
                Especialidad = c.Medico.Especialidad.ToString(),
                PacienteId = c.PacienteId,
                NombrePaciente = c.Paciente.Nombre,
                Fecha = c.Fecha,
                Hora = c.Hora,
                Motivo = c.Motivo,
                Estado = c.Estado.ToString(),
                DuracionMinutos = DuracionCitas.ObtenerDuracion(c.Medico.Especialidad)
            }).ToList();
        }

        private static void ValidarDatos(string nombre, string correo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del paciente es requerido.");

            if (string.IsNullOrWhiteSpace(correo))
                throw new ArgumentException("El correo electrónico del paciente es requerido.");
        }

        private static PacienteResponseDTO MapToResponse(Paciente paciente)
        {
            return new PacienteResponseDTO
            {
                Id = paciente.Id,
                Nombre = paciente.Nombre,
                FechaNacimiento = paciente.FechaNacimiento,
                Telefono = paciente.Telefono,
                CorreoElectronico = paciente.CorreoElectronico
            };
        }
    }
}
