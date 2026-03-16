using GACSE.Application.Interfaces.Repositories;
using GACSE.Domain.Entities;
using GACSE.Domain.Enums;
using GACSE.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GACSE.Infrastructure.Repositories
{
    public class CitaRepository : ICitaRepository
    {
        private readonly AppDbContext _context;

        public CitaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cita>> ObtenerTodosAsync()
        {
            return await _context.Citas
                .Include(c => c.Medico)
                .Include(c => c.Paciente)
                .AsNoTracking()
                .OrderByDescending(c => c.Fecha)
                .ThenBy(c => c.Hora)
                .ToListAsync();
        }

        public async Task<Cita?> ObtenerPorIdAsync(int id)
        {
            return await _context.Citas
                .Include(c => c.Medico)
                .Include(c => c.Paciente)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cita> CrearAsync(Cita cita)
        {
            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            // Recargar con navegación
            await _context.Entry(cita).Reference(c => c.Medico).LoadAsync();
            await _context.Entry(cita).Reference(c => c.Paciente).LoadAsync();

            return cita;
        }

        public async Task ActualizarAsync(Cita cita)
        {
            _context.Citas.Update(cita);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Cita>> ObtenerCitasPorMedicoYFechaAsync(int medicoId, DateTime fecha)
        {
            return await _context.Citas
                .Include(c => c.Medico)
                .Include(c => c.Paciente)
                .Where(c => c.MedicoId == medicoId && c.Fecha.Date == fecha.Date)
                .OrderBy(c => c.Hora)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Cita>> ObtenerCitasPorPacienteAsync(int pacienteId)
        {
            return await _context.Citas
                .Include(c => c.Medico)
                .Include(c => c.Paciente)
                .Where(c => c.PacienteId == pacienteId)
                .OrderByDescending(c => c.Fecha)
                .ThenBy(c => c.Hora)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ValidarDisponibilidadMedicoAsync(int medicoId, DateTime fecha, TimeSpan hora, int duracionMinutos)
        {
            var paramMedicoId = new SqlParameter("@MedicoId", medicoId);
            var paramFecha = new SqlParameter("@Fecha", fecha.Date);
            var paramHora = new SqlParameter("@Hora", hora);
            var paramDuracion = new SqlParameter("@DuracionMinutos", duracionMinutos);
            var paramDisponible = new SqlParameter("@Disponible", System.Data.SqlDbType.Bit)
            {
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_ValidarDisponibilidadMedico @MedicoId, @Fecha, @Hora, @DuracionMinutos, @Disponible OUTPUT",
                paramMedicoId, paramFecha, paramHora, paramDuracion, paramDisponible);

            return (bool)paramDisponible.Value;
        }

        public async Task<List<Cita>> ObtenerAgendaMedicoSpAsync(int medicoId, DateTime fecha)
        {
            var paramMedicoId = new SqlParameter("@MedicoId", medicoId);
            var paramFecha = new SqlParameter("@Fecha", fecha.Date);

            return await _context.Citas
                .FromSqlRaw("EXEC sp_ObtenerAgendaMedico @MedicoId, @Fecha", paramMedicoId, paramFecha)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> ContarCancelacionesRecientesAsync(int pacienteId, int dias)
        {
            var fechaLimite = DateTime.Now.AddDays(-dias);

            return await _context.Citas
                .Where(c => c.PacienteId == pacienteId
                         && c.Estado == EstadoCita.Cancelada
                         && c.Fecha >= fechaLimite)
                .CountAsync();
        }
    }
}
