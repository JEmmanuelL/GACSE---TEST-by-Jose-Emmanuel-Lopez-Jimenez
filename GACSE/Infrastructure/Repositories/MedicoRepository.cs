using GACSE.Application.Interfaces.Repositories;
using GACSE.Domain.Entities;
using GACSE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GACSE.Infrastructure.Repositories
{
    public class MedicoRepository : IMedicoRepository
    {
        private readonly AppDbContext _context;

        public MedicoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Medico>> ObtenerTodosAsync()
        {
            return await _context.Medicos
                .Include(m => m.Horarios)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Medico?> ObtenerPorIdAsync(int id)
        {
            return await _context.Medicos
                .Include(m => m.Horarios)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Medico?> ObtenerPorIdConHorariosAsync(int id)
        {
            return await _context.Medicos
                .Include(m => m.Horarios)
                .Include(m => m.Citas)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Medico> CrearAsync(Medico medico)
        {
            _context.Medicos.Add(medico);
            await _context.SaveChangesAsync();
            return medico;
        }

        public async Task ActualizarAsync(Medico medico)
        {
            _context.Medicos.Update(medico);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(Medico medico)
        {
            _context.Medicos.Remove(medico);
            await _context.SaveChangesAsync();
        }
    }
}
