using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GACSE.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Especialidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CorreoElectronico = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HorariosMedicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicoId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosMedicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosMedicos_Medicos_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicoId = table.Column<int>(type: "int", nullable: false),
                    PacienteId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hora = table.Column<TimeSpan>(type: "time", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Citas_Medicos_MedicoId",
                        column: x => x.MedicoId,
                        principalTable: "Medicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Medicos",
                columns: new[] { "Id", "Especialidad", "Nombre" },
                values: new object[,]
                {
                    { 1, "MedicinaGeneral", "Dr. Carlos García" },
                    { 2, "Cardiologia", "Dra. María López" },
                    { 3, "Cirugia", "Dr. Roberto Sánchez" },
                    { 4, "Pediatria", "Dra. Ana Martínez" }
                });

            migrationBuilder.InsertData(
                table: "Pacientes",
                columns: new[] { "Id", "CorreoElectronico", "FechaNacimiento", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, "juan.perez@email.com", new DateTime(1990, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Juan Pérez", "5551234567" },
                    { 2, "maria.rodriguez@email.com", new DateTime(1985, 8, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "María Rodríguez", "5559876543" },
                    { 3, "pedro.hernandez@email.com", new DateTime(2015, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pedro Hernández", "5554567890" },
                    { 4, "laura.gomez@email.com", new DateTime(1978, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Laura Gómez", "5557891234" },
                    { 5, "carlos.diaz@email.com", new DateTime(2000, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Carlos Díaz", "5552345678" },
                    { 6, "ana.torres@email.com", new DateTime(1995, 7, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ana Torres", "5558765432" }
                });

            migrationBuilder.InsertData(
                table: "Citas",
                columns: new[] { "Id", "Estado", "Fecha", "Hora", "MedicoId", "Motivo", "PacienteId" },
                values: new object[,]
                {
                    { 1, "Programada", new DateTime(2026, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 0, 0, 0), 1, "Consulta general", 1 },
                    { 2, "Programada", new DateTime(2026, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 8, 20, 0, 0), 1, "Dolor de cabeza frecuente", 2 },
                    { 3, "Programada", new DateTime(2026, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 9, 0, 0, 0), 2, "Revisión cardiológica", 4 },
                    { 4, "Programada", new DateTime(2026, 3, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 10, 0, 0, 0), 4, "Control pediátrico", 3 },
                    { 5, "Completada", new DateTime(2026, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 9, 0, 0, 0), 1, "Chequeo anual", 5 }
                });

            migrationBuilder.InsertData(
                table: "HorariosMedicos",
                columns: new[] { "Id", "DiaSemana", "HoraFin", "HoraInicio", "MedicoId" },
                values: new object[,]
                {
                    { 1, 1, new TimeSpan(0, 14, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 1 },
                    { 2, 2, new TimeSpan(0, 14, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 1 },
                    { 3, 3, new TimeSpan(0, 14, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 1 },
                    { 4, 4, new TimeSpan(0, 14, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 1 },
                    { 5, 5, new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 1 },
                    { 6, 1, new TimeSpan(0, 15, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 2 },
                    { 7, 3, new TimeSpan(0, 15, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 2 },
                    { 8, 5, new TimeSpan(0, 13, 0, 0, 0), new TimeSpan(0, 9, 0, 0, 0), 2 },
                    { 9, 2, new TimeSpan(0, 13, 0, 0, 0), new TimeSpan(0, 7, 0, 0, 0), 3 },
                    { 10, 4, new TimeSpan(0, 13, 0, 0, 0), new TimeSpan(0, 7, 0, 0, 0), 3 },
                    { 11, 1, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 4 },
                    { 12, 2, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 4 },
                    { 13, 3, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 4 },
                    { 14, 4, new TimeSpan(0, 16, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 4 },
                    { 15, 5, new TimeSpan(0, 14, 0, 0, 0), new TimeSpan(0, 8, 0, 0, 0), 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_MedicoId_Fecha_Hora",
                table: "Citas",
                columns: new[] { "MedicoId", "Fecha", "Hora" });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_PacienteId",
                table: "Citas",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosMedicos_MedicoId_DiaSemana",
                table: "HorariosMedicos",
                columns: new[] { "MedicoId", "DiaSemana" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "HorariosMedicos");

            migrationBuilder.DropTable(
                name: "Pacientes");

            migrationBuilder.DropTable(
                name: "Medicos");
        }
    }
}
