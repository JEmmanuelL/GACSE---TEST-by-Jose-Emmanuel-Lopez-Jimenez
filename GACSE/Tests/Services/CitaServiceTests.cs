using GACSE.Application.DTOs;
using GACSE.Application.Interfaces.Repositories;
using GACSE.Application.Interfaces.Services;
using GACSE.Application.Services;
using GACSE.Domain.Constants;
using GACSE.Domain.Entities;
using GACSE.Domain.Enums;
using Moq;
using Xunit;

namespace GACSE.Tests.Services
{
    public class CitaServiceTests
    {
        private readonly Mock<ICitaRepository> _citaRepoMock;
        private readonly Mock<IMedicoRepository> _medicoRepoMock;
        private readonly Mock<IPacienteRepository> _pacienteRepoMock;
        private readonly Mock<IMedicoService> _medicoServiceMock;
        private readonly CitaService _citaService;

        public CitaServiceTests()
        {
            _citaRepoMock = new Mock<ICitaRepository>();
            _medicoRepoMock = new Mock<IMedicoRepository>();
            _pacienteRepoMock = new Mock<IPacienteRepository>();
            _medicoServiceMock = new Mock<IMedicoService>();

            _citaService = new CitaService(
                _citaRepoMock.Object,
                _medicoRepoMock.Object,
                _pacienteRepoMock.Object,
                _medicoServiceMock.Object);
        }

        private static Medico CrearMedicoConHorario(
            int id = 1,
            EspecialidadMedica especialidad = EspecialidadMedica.MedicinaGeneral,
            DayOfWeek dia = DayOfWeek.Monday)
        {
            return new Medico
            {
                Id = id,
                Nombre = "Dr. Test",
                Especialidad = especialidad,
                Horarios = new List<HorarioMedico>
                {
                    new HorarioMedico
                    {
                        Id = 1,
                        MedicoId = id,
                        DiaSemana = dia,
                        HoraInicio = new TimeSpan(8, 0, 0),
                        HoraFin = new TimeSpan(14, 0, 0)
                    }
                }
            };
        }

        private static Paciente CrearPaciente(int id = 1)
        {
            return new Paciente
            {
                Id = id,
                Nombre = "Paciente Test",
                FechaNacimiento = new DateTime(1990, 1, 1),
                Telefono = "5551234567",
                CorreoElectronico = "test@email.com"
            };
        }

        // Buscar el próximo día de la semana a partir de hoy
        private static DateTime ObtenerProximaFecha(DayOfWeek dia)
        {
            var fecha = DateTime.Now.Date.AddDays(1);
            while (fecha.DayOfWeek != dia)
                fecha = fecha.AddDays(1);
            return fecha;
        }

        // ═══════════════════════════════════════════════
        // TEST 1: Conflicto de horarios → InvalidOperationException
        // ═══════════════════════════════════════════════
        [Fact]
        public async Task AgendarCita_ConConflictoDeHorario_LanzaExcepcion()
        {
            // Arrange
            var fecha = ObtenerProximaFecha(DayOfWeek.Monday);
            var medico = CrearMedicoConHorario(dia: DayOfWeek.Monday);
            var paciente = CrearPaciente();

            _medicoRepoMock.Setup(r => r.ObtenerPorIdConHorariosAsync(1)).ReturnsAsync(medico);
            _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(paciente);

            // Ya existe una cita a las 08:00
            var citaExistente = new Cita
            {
                Id = 1, MedicoId = 1, PacienteId = 2,
                Fecha = fecha, Hora = new TimeSpan(8, 0, 0),
                Motivo = "Existente", Estado = EstadoCita.Programada,
                Medico = medico, Paciente = paciente
            };

            _citaRepoMock.Setup(r => r.ObtenerCitasPorMedicoYFechaAsync(1, fecha))
                .ReturnsAsync(new List<Cita> { citaExistente });

            _medicoServiceMock.Setup(s => s.ObtenerHorariosDisponiblesAsync(1, fecha))
                .ReturnsAsync(new List<HorarioDisponibleDTO>());

            var dto = new AgendarCitaDTO
            {
                MedicoId = 1, PacienteId = 1,
                Fecha = fecha,
                Hora = new TimeSpan(8, 0, 0), // Misma hora → conflicto
                Motivo = "Consulta"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _citaService.AgendarCitaAsync(dto));
        }

        // ═══════════════════════════════════════════════
        // TEST 2: Duración por especialidad es correcta
        // ═══════════════════════════════════════════════
        [Theory]
        [InlineData(EspecialidadMedica.MedicinaGeneral, 20)]
        [InlineData(EspecialidadMedica.Cardiologia, 30)]
        [InlineData(EspecialidadMedica.Cirugia, 45)]
        [InlineData(EspecialidadMedica.Pediatria, 20)]
        [InlineData(EspecialidadMedica.Ginecologia, 30)]
        public void DuracionPorEspecialidad_RetornaValorCorrecto(EspecialidadMedica especialidad, int duracionEsperada)
        {
            // Act
            var duracion = DuracionCitas.ObtenerDuracion(especialidad);

            // Assert
            Assert.Equal(duracionEsperada, duracion);
        }

        // ═══════════════════════════════════════════════
        // TEST 3: Cita en el pasado → ArgumentException
        // ═══════════════════════════════════════════════
        [Fact]
        public async Task AgendarCita_EnElPasado_LanzaArgumentException()
        {
            // Arrange
            var medico = CrearMedicoConHorario(dia: DayOfWeek.Monday);
            var paciente = CrearPaciente();

            _medicoRepoMock.Setup(r => r.ObtenerPorIdConHorariosAsync(1)).ReturnsAsync(medico);
            _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(paciente);

            var dto = new AgendarCitaDTO
            {
                MedicoId = 1, PacienteId = 1,
                Fecha = new DateTime(2020, 1, 6), // Fecha pasada (lunes)
                Hora = new TimeSpan(8, 0, 0),
                Motivo = "Consulta"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _citaService.AgendarCitaAsync(dto));
            Assert.Contains("pasaron", ex.Message);
        }

        // ═══════════════════════════════════════════════
        // TEST 4: Alerta de cancelaciones (3+ en 30 días)
        // ═══════════════════════════════════════════════
        [Fact]
        public async Task AgendarCita_ConAlertaDeCancelaciones_IncluyeAlerta()
        {
            // Arrange
            var fecha = ObtenerProximaFecha(DayOfWeek.Monday);
            var medico = CrearMedicoConHorario(dia: DayOfWeek.Monday);
            var paciente = CrearPaciente();

            _medicoRepoMock.Setup(r => r.ObtenerPorIdConHorariosAsync(1)).ReturnsAsync(medico);
            _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(paciente);
            _citaRepoMock.Setup(r => r.ObtenerCitasPorMedicoYFechaAsync(1, fecha))
                .ReturnsAsync(new List<Cita>()); // Sin conflictos

            // 3 cancelaciones en últimos 30 días
            _citaRepoMock.Setup(r => r.ContarCancelacionesRecientesAsync(1, 30)).ReturnsAsync(3);

            var citaCreada = new Cita
            {
                Id = 10, MedicoId = 1, PacienteId = 1,
                Fecha = fecha, Hora = new TimeSpan(8, 0, 0),
                Motivo = "Consulta", Estado = EstadoCita.Programada,
                Medico = medico, Paciente = paciente
            };
            _citaRepoMock.Setup(r => r.CrearAsync(It.IsAny<Cita>())).ReturnsAsync(citaCreada);

            var dto = new AgendarCitaDTO
            {
                MedicoId = 1, PacienteId = 1,
                Fecha = fecha,
                Hora = new TimeSpan(8, 0, 0),
                Motivo = "Consulta"
            };

            // Act
            var resultado = await _citaService.AgendarCitaAsync(dto);

            // Assert
            Assert.True(resultado.AlertaCancelaciones);
            Assert.Contains("cancelaciones", resultado.MensajeAlerta!);
        }

        // ═══════════════════════════════════════════════
        // TEST 5: Sugerencia de horarios cuando hay conflicto
        // ═══════════════════════════════════════════════
        [Fact]
        public async Task AgendarCita_ConConflicto_SugiereHorarios()
        {
            // Arrange
            var fecha = ObtenerProximaFecha(DayOfWeek.Monday);
            var medico = CrearMedicoConHorario(dia: DayOfWeek.Monday);
            var paciente = CrearPaciente();

            _medicoRepoMock.Setup(r => r.ObtenerPorIdConHorariosAsync(1)).ReturnsAsync(medico);
            _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(paciente);

            var citaExistente = new Cita
            {
                Id = 1, MedicoId = 1, PacienteId = 2,
                Fecha = fecha, Hora = new TimeSpan(9, 0, 0),
                Motivo = "Existente", Estado = EstadoCita.Programada,
                Medico = medico, Paciente = paciente
            };

            _citaRepoMock.Setup(r => r.ObtenerCitasPorMedicoYFechaAsync(1, fecha))
                .ReturnsAsync(new List<Cita> { citaExistente });

            // Simular 5 horarios sugeridos
            var horariosSugeridos = new List<HorarioDisponibleDTO>
            {
                new() { Fecha = fecha, Hora = new TimeSpan(8, 0, 0), DiaSemana = "Lunes" },
                new() { Fecha = fecha, Hora = new TimeSpan(8, 20, 0), DiaSemana = "Lunes" },
                new() { Fecha = fecha, Hora = new TimeSpan(8, 40, 0), DiaSemana = "Lunes" },
                new() { Fecha = fecha, Hora = new TimeSpan(9, 20, 0), DiaSemana = "Lunes" },
                new() { Fecha = fecha, Hora = new TimeSpan(9, 40, 0), DiaSemana = "Lunes" }
            };

            _medicoServiceMock.Setup(s => s.ObtenerHorariosDisponiblesAsync(1, fecha))
                .ReturnsAsync(horariosSugeridos);

            var dto = new AgendarCitaDTO
            {
                MedicoId = 1, PacienteId = 1,
                Fecha = fecha,
                Hora = new TimeSpan(9, 0, 0), // Conflicto con existente
                Motivo = "Consulta"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _citaService.AgendarCitaAsync(dto));
            Assert.Contains("disponible", ex.Message);
            Assert.Contains("horarios", ex.Message.ToLower());
        }

        // ═══════════════════════════════════════════════
        // TEST 6: Cita fuera de horario del médico
        // ═══════════════════════════════════════════════
        [Fact]
        public async Task AgendarCita_FueraDeHorario_LanzaArgumentException()
        {
            // Arrange
            var fecha = ObtenerProximaFecha(DayOfWeek.Monday);
            var medico = CrearMedicoConHorario(dia: DayOfWeek.Monday); // 08:00-14:00
            var paciente = CrearPaciente();

            _medicoRepoMock.Setup(r => r.ObtenerPorIdConHorariosAsync(1)).ReturnsAsync(medico);
            _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(paciente);

            var dto = new AgendarCitaDTO
            {
                MedicoId = 1, PacienteId = 1,
                Fecha = fecha,
                Hora = new TimeSpan(15, 0, 0), // Fuera de horario (14:00 max)
                Motivo = "Consulta"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _citaService.AgendarCitaAsync(dto));
            Assert.Contains("horario", ex.Message.ToLower());
        }

        // ═══════════════════════════════════════════════
        // TEST 7: Cita que no cabe completa antes del fin del horario
        // ═══════════════════════════════════════════════
        [Fact]
        public async Task AgendarCita_NoCabeComletaAntesDeCierre_LanzaArgumentException()
        {
            // Arrange — Cardiólogo con cita de 30 min, horario hasta 14:00
            var fecha = ObtenerProximaFecha(DayOfWeek.Monday);
            var medico = CrearMedicoConHorario(especialidad: EspecialidadMedica.Cardiologia, dia: DayOfWeek.Monday);
            var paciente = CrearPaciente();

            _medicoRepoMock.Setup(r => r.ObtenerPorIdConHorariosAsync(1)).ReturnsAsync(medico);
            _pacienteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(paciente);

            var dto = new AgendarCitaDTO
            {
                MedicoId = 1, PacienteId = 1,
                Fecha = fecha,
                Hora = new TimeSpan(13, 45, 0), // 13:45 + 30 min = 14:15 > 14:00
                Motivo = "Consulta"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _citaService.AgendarCitaAsync(dto));
            Assert.Contains("no cabe", ex.Message.ToLower());
        }
    }
}
