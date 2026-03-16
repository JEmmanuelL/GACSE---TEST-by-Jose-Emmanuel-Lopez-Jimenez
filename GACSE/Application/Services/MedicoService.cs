using GACSE.Application.DTOs;
using GACSE.Application.Interfaces.Repositories;
using GACSE.Application.Interfaces.Services;
using GACSE.Domain.Constants;
using GACSE.Domain.Entities;
using GACSE.Domain.Enums;

namespace GACSE.Application.Services
{
    public class MedicoService : IMedicoService
    {
        private readonly IMedicoRepository _medicoRepository;
        private readonly ICitaRepository _citaRepository;

        public MedicoService(IMedicoRepository medicoRepository, ICitaRepository citaRepository)
        {
            _medicoRepository = medicoRepository;
            _citaRepository = citaRepository;
        }

        public async Task<List<MedicoResponseDTO>> ObtenerTodosAsync()
        {
            var medicos = await _medicoRepository.ObtenerTodosAsync();
            return medicos.Select(MapToResponse).ToList();
        }

        public async Task<MedicoResponseDTO> ObtenerPorIdAsync(int id)
        {
            var medico = await _medicoRepository.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró el médico con Id {id}.");
            return MapToResponse(medico);
        }

        public async Task<MedicoResponseDTO> CrearAsync(CrearMedicoDTO dto)
        {
            ValidarDatos(dto.Nombre, dto.Horarios);

            var medico = new Medico
            {
                Nombre = dto.Nombre,
                Especialidad = dto.Especialidad,
                Horarios = dto.Horarios.Select(h => new HorarioMedico
                {
                    DiaSemana = h.DiaSemana,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin
                }).ToList()
            };

            await _medicoRepository.CrearAsync(medico);
            return MapToResponse(medico);
        }

        public async Task<MedicoResponseDTO> ActualizarAsync(int id, ActualizarMedicoDTO dto)
        {
            var medico = await _medicoRepository.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró el médico con Id {id}.");

            ValidarDatos(dto.Nombre, dto.Horarios);

            medico.Nombre = dto.Nombre;
            medico.Especialidad = dto.Especialidad;

            // Reemplazar horarios
            medico.Horarios.Clear();
            foreach (var h in dto.Horarios)
            {
                medico.Horarios.Add(new HorarioMedico
                {
                    DiaSemana = h.DiaSemana,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin
                });
            }

            await _medicoRepository.ActualizarAsync(medico);
            return MapToResponse(medico);
        }

        public async Task EliminarAsync(int id)
        {
            var medico = await _medicoRepository.ObtenerPorIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró el médico con Id {id}.");
            await _medicoRepository.EliminarAsync(medico);
        }

        public async Task<List<HorarioDisponibleDTO>> ObtenerHorariosDisponiblesAsync(int medicoId, DateTime fecha)
        {
            var medico = await _medicoRepository.ObtenerPorIdConHorariosAsync(medicoId)
                ?? throw new KeyNotFoundException($"No se encontró el médico con Id {medicoId}.");

            var duracion = DuracionCitas.ObtenerDuracion(medico.Especialidad);
            var horariosDisponibles = new List<HorarioDisponibleDTO>();
            var fechaActual = fecha.Date;
            var ahora = DateTime.Now;

            // Buscar en los próximos 30 días hasta encontrar 5 slots disponibles
            for (int dia = 0; dia < 30 && horariosDisponibles.Count < 5; dia++)
            {
                var fechaBusqueda = fechaActual.AddDays(dia);
                var diaSemana = fechaBusqueda.DayOfWeek;

                var horario = medico.Horarios.FirstOrDefault(h => h.DiaSemana == diaSemana);
                if (horario == null) continue;

                // Obtener citas existentes para ese día
                var citasDelDia = await _citaRepository.ObtenerCitasPorMedicoYFechaAsync(medicoId, fechaBusqueda);
                var citasActivas = citasDelDia.Where(c => c.Estado != EstadoCita.Cancelada).ToList();

                // Generar slots disponibles cada 10 minutos
                var slot = horario.HoraInicio;
                var ultimoSlot = horario.HoraFin.Subtract(TimeSpan.FromMinutes(duracion));
                var incremento = TimeSpan.FromMinutes(10);

                while (slot <= ultimoSlot && horariosDisponibles.Count < 5)
                {
                    var slotFin = slot.Add(TimeSpan.FromMinutes(duracion));

                    // Validar que no sea en el pasado
                    var fechaHoraSlot = fechaBusqueda.Add(slot);
                    if (fechaHoraSlot <= ahora)
                    {
                        slot = slot.Add(incremento);
                        continue;
                    }

                    // Verificar que no haya conflicto
                    bool hayConflicto = citasActivas.Any(c =>
                    {
                        var citaDuracion = DuracionCitas.ObtenerDuracion(medico.Especialidad);
                        var citaFin = c.Hora.Add(TimeSpan.FromMinutes(citaDuracion));
                        return slot < citaFin && slotFin > c.Hora;
                    });

                    if (!hayConflicto)
                    {
                        horariosDisponibles.Add(new HorarioDisponibleDTO
                        {
                            Fecha = fechaBusqueda,
                            Hora = slot,
                            DiaSemana = ObtenerNombreDia(diaSemana)
                        });
                    }

                    slot = slot.Add(incremento);
                }
            }

            return horariosDisponibles;
        }

        private static void ValidarDatos(string nombre, List<HorarioMedicoDTO> horarios)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del médico es requerido.");

            foreach (var h in horarios)
            {
                if (h.HoraInicio >= h.HoraFin)
                    throw new ArgumentException($"La hora de inicio debe ser anterior a la hora de fin para el día {ObtenerNombreDia(h.DiaSemana)}.");
            }
        }

        private static MedicoResponseDTO MapToResponse(Medico medico)
        {
            return new MedicoResponseDTO
            {
                Id = medico.Id,
                Nombre = medico.Nombre,
                Especialidad = medico.Especialidad.ToString(),
                Horarios = medico.Horarios.Select(h => new HorarioMedicoDTO
                {
                    DiaSemana = h.DiaSemana,
                    HoraInicio = h.HoraInicio,
                    HoraFin = h.HoraFin
                }).ToList()
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
