using GACSE.Domain.Entities;
using GACSE.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GACSE.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Medico> Medicos => Set<Medico>();
        public DbSet<Paciente> Pacientes => Set<Paciente>();
        public DbSet<Cita> Citas => Set<Cita>();
        public DbSet<HorarioMedico> HorariosMedicos => Set<HorarioMedico>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Medico ──
            modelBuilder.Entity<Medico>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(m => m.Especialidad)
                      .IsRequired()
                      .HasConversion<string>()
                      .HasMaxLength(50);

                entity.HasMany(m => m.Horarios)
                      .WithOne(h => h.Medico)
                      .HasForeignKey(h => h.MedicoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(m => m.Citas)
                      .WithOne(c => c.Medico)
                      .HasForeignKey(c => c.MedicoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── HorarioMedico ──
            modelBuilder.Entity<HorarioMedico>(entity =>
            {
                entity.HasKey(h => h.Id);
                entity.Property(h => h.DiaSemana).IsRequired();
                entity.Property(h => h.HoraInicio).IsRequired();
                entity.Property(h => h.HoraFin).IsRequired();

                // Un médico no puede tener dos horarios para el mismo día
                entity.HasIndex(h => new { h.MedicoId, h.DiaSemana }).IsUnique();
            });

            // ── Paciente ──
            modelBuilder.Entity<Paciente>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Telefono).HasMaxLength(20);
                entity.Property(p => p.CorreoElectronico).HasMaxLength(200);

                entity.HasMany(p => p.Citas)
                      .WithOne(c => c.Paciente)
                      .HasForeignKey(c => c.PacienteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Cita ──
            modelBuilder.Entity<Cita>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Motivo).IsRequired().HasMaxLength(500);
                entity.Property(c => c.Estado)
                      .IsRequired()
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(c => c.Fecha).IsRequired();
                entity.Property(c => c.Hora).IsRequired();

                // Índice para prevenir citas duplicadas del mismo médico en la misma fecha/hora
                entity.HasIndex(c => new { c.MedicoId, c.Fecha, c.Hora });
            });

            // ── Seed Data ──
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Médicos
            modelBuilder.Entity<Medico>().HasData(
                new Medico { Id = 1, Nombre = "Dr. Carlos García", Especialidad = EspecialidadMedica.MedicinaGeneral },
                new Medico { Id = 2, Nombre = "Dra. María López", Especialidad = EspecialidadMedica.Cardiologia },
                new Medico { Id = 3, Nombre = "Dr. Roberto Sánchez", Especialidad = EspecialidadMedica.Cirugia },
                new Medico { Id = 4, Nombre = "Dra. Ana Martínez", Especialidad = EspecialidadMedica.Pediatria }
            );

            // Horarios de médicos
            modelBuilder.Entity<HorarioMedico>().HasData(
                // Dr. Carlos García - Medicina General
                new HorarioMedico { Id = 1, MedicoId = 1, DiaSemana = DayOfWeek.Monday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(14, 0, 0) },
                new HorarioMedico { Id = 2, MedicoId = 1, DiaSemana = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(14, 0, 0) },
                new HorarioMedico { Id = 3, MedicoId = 1, DiaSemana = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(14, 0, 0) },
                new HorarioMedico { Id = 4, MedicoId = 1, DiaSemana = DayOfWeek.Thursday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(14, 0, 0) },
                new HorarioMedico { Id = 5, MedicoId = 1, DiaSemana = DayOfWeek.Friday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(12, 0, 0) },

                // Dra. María López - Cardiología
                new HorarioMedico { Id = 6, MedicoId = 2, DiaSemana = DayOfWeek.Monday, HoraInicio = new TimeSpan(9, 0, 0), HoraFin = new TimeSpan(15, 0, 0) },
                new HorarioMedico { Id = 7, MedicoId = 2, DiaSemana = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(9, 0, 0), HoraFin = new TimeSpan(15, 0, 0) },
                new HorarioMedico { Id = 8, MedicoId = 2, DiaSemana = DayOfWeek.Friday, HoraInicio = new TimeSpan(9, 0, 0), HoraFin = new TimeSpan(13, 0, 0) },

                // Dr. Roberto Sánchez - Cirugía
                new HorarioMedico { Id = 9, MedicoId = 3, DiaSemana = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(7, 0, 0), HoraFin = new TimeSpan(13, 0, 0) },
                new HorarioMedico { Id = 10, MedicoId = 3, DiaSemana = DayOfWeek.Thursday, HoraInicio = new TimeSpan(7, 0, 0), HoraFin = new TimeSpan(13, 0, 0) },

                // Dra. Ana Martínez - Pediatría
                new HorarioMedico { Id = 11, MedicoId = 4, DiaSemana = DayOfWeek.Monday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(16, 0, 0) },
                new HorarioMedico { Id = 12, MedicoId = 4, DiaSemana = DayOfWeek.Tuesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(16, 0, 0) },
                new HorarioMedico { Id = 13, MedicoId = 4, DiaSemana = DayOfWeek.Wednesday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(16, 0, 0) },
                new HorarioMedico { Id = 14, MedicoId = 4, DiaSemana = DayOfWeek.Thursday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(16, 0, 0) },
                new HorarioMedico { Id = 15, MedicoId = 4, DiaSemana = DayOfWeek.Friday, HoraInicio = new TimeSpan(8, 0, 0), HoraFin = new TimeSpan(14, 0, 0) }
            );

            // Pacientes
            modelBuilder.Entity<Paciente>().HasData(
                new Paciente { Id = 1, Nombre = "Juan Pérez", FechaNacimiento = new DateTime(1990, 5, 15), Telefono = "5551234567", CorreoElectronico = "juan.perez@email.com" },
                new Paciente { Id = 2, Nombre = "María Rodríguez", FechaNacimiento = new DateTime(1985, 8, 22), Telefono = "5559876543", CorreoElectronico = "maria.rodriguez@email.com" },
                new Paciente { Id = 3, Nombre = "Pedro Hernández", FechaNacimiento = new DateTime(2015, 3, 10), Telefono = "5554567890", CorreoElectronico = "pedro.hernandez@email.com" },
                new Paciente { Id = 4, Nombre = "Laura Gómez", FechaNacimiento = new DateTime(1978, 11, 30), Telefono = "5557891234", CorreoElectronico = "laura.gomez@email.com" },
                new Paciente { Id = 5, Nombre = "Carlos Díaz", FechaNacimiento = new DateTime(2000, 1, 5), Telefono = "5552345678", CorreoElectronico = "carlos.diaz@email.com" },
                new Paciente { Id = 6, Nombre = "Ana Torres", FechaNacimiento = new DateTime(1995, 7, 18), Telefono = "5558765432", CorreoElectronico = "ana.torres@email.com" }
            );

            // Citas de ejemplo (fechas futuras relativas)
            modelBuilder.Entity<Cita>().HasData(
                new Cita { Id = 1, MedicoId = 1, PacienteId = 1, Fecha = new DateTime(2026, 3, 23), Hora = new TimeSpan(8, 0, 0), Motivo = "Consulta general", Estado = EstadoCita.Programada },
                new Cita { Id = 2, MedicoId = 1, PacienteId = 2, Fecha = new DateTime(2026, 3, 23), Hora = new TimeSpan(8, 20, 0), Motivo = "Dolor de cabeza frecuente", Estado = EstadoCita.Programada },
                new Cita { Id = 3, MedicoId = 2, PacienteId = 4, Fecha = new DateTime(2026, 3, 23), Hora = new TimeSpan(9, 0, 0), Motivo = "Revisión cardiológica", Estado = EstadoCita.Programada },
                new Cita { Id = 4, MedicoId = 4, PacienteId = 3, Fecha = new DateTime(2026, 3, 23), Hora = new TimeSpan(10, 0, 0), Motivo = "Control pediátrico", Estado = EstadoCita.Programada },
                new Cita { Id = 5, MedicoId = 1, PacienteId = 5, Fecha = new DateTime(2026, 3, 16), Hora = new TimeSpan(9, 0, 0), Motivo = "Chequeo anual", Estado = EstadoCita.Completada }
            );
        }
    }
}
