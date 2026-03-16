using GACSE.Application.DTOs;
using GACSE.Application.Interfaces.Repositories;
using GACSE.Application.Interfaces.Services;
using GACSE.Domain.Constants;
using GACSE.Domain.Entities;
using GACSE.Domain.Enums;

namespace GACSE.Application.Services
{
    public class CitaService : ICitaService
    {
        private readonly ICitaRepository _citaRepository;
        private readonly IMedicoRepository _medicoRepository;
        private readonly IPacienteRepository _pacienteRepository;
        private readonly IMedicoService _medicoService;

        public CitaService(
            ICitaRepository citaRepository,
            IMedicoRepository medicoRepository,
            IPacienteRepository pacienteRepository,
            IMedicoService medicoService)
        {
            _citaRepository = citaRepository;
            _medicoRepository = medicoRepository;
            _pacienteRepository = pacienteRepository;
            _medicoService = medicoService;
        }

        public async Task<AgendarCitaResponseDTO> AgendarCitaAsync(AgendarCitaDTO dto)
        {
            // 1. Validar que el médico existe
            var medico = await _medicoRepository.ObtenerPorIdConHorariosAsync(dto.MedicoId)
                ?? throw new KeyNotFoundException($"No se encontró el médico con Id {dto.MedicoId}.");

            // 2. Validar que el paciente existe
            var paciente = await _pacienteRepository.ObtenerPorIdAsync(dto.PacienteId)
                ?? throw new KeyNotFoundException($"No se encontró el paciente con Id {dto.PacienteId}.");

            // 3. Validar que no es una fecha/hora en el pasado
            var fechaHoraCita = dto.Fecha.Date.Add(dto.Hora);
            if (fechaHoraCita <= DateTime.Now)
                throw new ArgumentException("No se pueden agendar citas en fechas u horas que ya pasaron.");

            // 4. Validar motivo
            if (string.IsNullOrWhiteSpace(dto.Motivo))
                throw new ArgumentException("El motivo de la cita es requerido.");

            // 5. Obtener duración por especialidad
            var duracionMinutos = DuracionCitas.ObtenerDuracion(medico.Especialidad);

            // 6. Validar que la cita esté dentro del horario del médico
            var diaSemana = dto.Fecha.DayOfWeek;
            var horarioDelDia = medico.Horarios.FirstOrDefault(h => h.DiaSemana == diaSemana);

            if (horarioDelDia == null)
                throw new ArgumentException($"El médico no tiene horario de consulta el día {ObtenerNombreDia(diaSemana)}.");

            if (dto.Hora < horarioDelDia.HoraInicio || dto.Hora >= horarioDelDia.HoraFin)
                throw new ArgumentException(
                    $"La hora de la cita debe estar dentro del horario del médico: {horarioDelDia.HoraInicio:hh\\:mm} - {horarioDelDia.HoraFin:hh\\:mm}.");

            // 7. Validar que la cita completa quepa antes del fin del horario
            var horaFinCita = dto.Hora.Add(TimeSpan.FromMinutes(duracionMinutos));
            if (horaFinCita > horarioDelDia.HoraFin)
                throw new ArgumentException(
                    $"La cita de {duracionMinutos} minutos no cabe en el horario. La última hora posible es {horarioDelDia.HoraFin.Subtract(TimeSpan.FromMinutes(duracionMinutos)):hh\\:mm}.");

            // 8. Validar conflicto de horario (sin citas simultáneas)
            var citasDelDia = await _citaRepository.ObtenerCitasPorMedicoYFechaAsync(dto.MedicoId, dto.Fecha);
            var citasActivas = citasDelDia.Where(c => c.Estado != EstadoCita.Cancelada).ToList();

            bool hayConflicto = citasActivas.Any(c =>
            {
                var duracionExistente = DuracionCitas.ObtenerDuracion(medico.Especialidad);
                var finExistente = c.Hora.Add(TimeSpan.FromMinutes(duracionExistente));

                // Hay conflicto si los rangos se solapan
                return dto.Hora < finExistente && horaFinCita > c.Hora;
            });

            if (hayConflicto)
            {
                // Sugerencia de horarios: devolver los próximos 5 disponibles
                var horariosDisponibles = await _medicoService.ObtenerHorariosDisponiblesAsync(dto.MedicoId, dto.Fecha);

                throw new InvalidOperationException(
                    System.Text.Json.JsonSerializer.Serialize(new ConflictoHorarioResponseDTO
                    {
                        Mensaje = "El horario solicitado no está disponible. Se sugieren los siguientes horarios.",
                        HorariosDisponibles = horariosDisponibles
                    }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
            }

            // 9. Verificar alerta de cancelaciones (3+ en últimos 30 días)
            var cancelaciones = await _citaRepository.ContarCancelacionesRecientesAsync(dto.PacienteId, 30);
            bool alertaCancelaciones = cancelaciones >= 3;

            // 10. Crear la cita
            var cita = new Cita
            {
                MedicoId = dto.MedicoId,
                PacienteId = dto.PacienteId,
                Fecha = dto.Fecha.Date,
                Hora = dto.Hora,
                Motivo = dto.Motivo,
                Estado = EstadoCita.Programada
            };

            var citaCreada = await _citaRepository.CrearAsync(cita);

            return new AgendarCitaResponseDTO
            {
                Cita = MapToResponse(citaCreada, medico.Especialidad),
                AlertaCancelaciones = alertaCancelaciones,
                MensajeAlerta = alertaCancelaciones
                    ? $"Alerta: El paciente tiene {cancelaciones} cancelaciones en los últimos 30 días."
                    : null
            };
        }

        public async Task<CitaResponseDTO> CancelarCitaAsync(int id)
        {
            var cita = await _citaRepository.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró la cita con Id {id}.");

            if (cita.Estado == EstadoCita.Cancelada)
                throw new ArgumentException("La cita ya se encuentra cancelada.");

            if (cita.Estado == EstadoCita.Completada)
                throw new ArgumentException("No se puede cancelar una cita que ya fue completada.");

            cita.Estado = EstadoCita.Cancelada;
            await _citaRepository.ActualizarAsync(cita);

            return MapToResponse(cita, cita.Medico.Especialidad);
        }

        public async Task<List<CitaResponseDTO>> ConsultarCitasAsync()
        {
            var citas = await _citaRepository.ObtenerTodosAsync();
            return citas.Select(c => MapToResponse(c, c.Medico.Especialidad)).ToList();
        }

        public async Task<AgendaDiaDTO> ObtenerAgendaDelDiaAsync(int medicoId, DateTime fecha)
        {
            var medico = await _medicoRepository.ObtenerPorIdAsync(medicoId)
                ?? throw new KeyNotFoundException($"No se encontró el médico con Id {medicoId}.");

            // Usar stored procedure para obtener agenda
            List<Cita> citas;
            try
            {
                citas = await _citaRepository.ObtenerAgendaMedicoSpAsync(medicoId, fecha);
                // El SP no carga relaciones de navegación, recargar con EF si hay citas
                if (citas.Any(c => c.Paciente == null))
                {
                    citas = await _citaRepository.ObtenerCitasPorMedicoYFechaAsync(medicoId, fecha);
                }
            }
            catch
            {
                // Fallback a EF Core si el SP no está disponible
                citas = await _citaRepository.ObtenerCitasPorMedicoYFechaAsync(medicoId, fecha);
            }

            return new AgendaDiaDTO
            {
                MedicoId = medico.Id,
                NombreMedico = medico.Nombre,
                Fecha = fecha,
                Citas = citas.Select(c => MapToResponse(c, medico.Especialidad)).ToList()
            };
        }

        private static CitaResponseDTO MapToResponse(Cita cita, EspecialidadMedica especialidad)
        {
            return new CitaResponseDTO
            {
                Id = cita.Id,
                MedicoId = cita.MedicoId,
                NombreMedico = cita.Medico?.Nombre ?? string.Empty,
                Especialidad = especialidad.ToString(),
                PacienteId = cita.PacienteId,
                NombrePaciente = cita.Paciente?.Nombre ?? string.Empty,
                Fecha = cita.Fecha,
                Hora = cita.Hora,
                Motivo = cita.Motivo,
                Estado = cita.Estado.ToString(),
                DuracionMinutos = DuracionCitas.ObtenerDuracion(especialidad)
            };
        }

        private static string ObtenerNombreDia(DayOfWeek dia) => dia switch
        {
            DayOfWeek.Monday => "Lunes",
            DayOfWeek.Tuesday => "Martes",
            DayOfWeek.Wednesday => "Miércoles",
            DayOfWeek.Thursday => "Jueves",
            DayOfWeek.Friday => "Viernes",
            DayOfWeek.Saturday => "Sábado",
            DayOfWeek.Sunday => "Domingo",
            _ => dia.ToString()
        };
    }
}
