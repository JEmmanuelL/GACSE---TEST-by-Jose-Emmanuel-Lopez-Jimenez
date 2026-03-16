# MIGRACION.md - Gestión de Base de Datos (EF Core & SQL)

Este documento describe el proceso técnico de migración y actualización de la base de datos del proyecto **GACSE** utilizando **Entity Framework Core** y scripts **SQL** nativos.

## 1. Migraciones de Entity Framework Core
El proyecto utiliza el enfoque *Code-First* para definir el esquema de la base de datos. 

### Aplicación de Migraciones
Las migraciones se aplican automáticamente al iniciar la aplicación en `Program.cs` mediante:
```csharp
db.Database.Migrate();
```
Esto asegura que las tablas `Medicos`, `Pacientes`, `Citas` y `HorariosMedicos` se creen o actualicen según las clases definidas en la capa de **Domain**.

### Comandos de Desarrollo
Si se realizan cambios en las entidades de dominio, se deben generar nuevas migraciones:
```bash
dotnet ef migrations add NombreDeLaMigracion --project Infrastructure --startup-project GACSE
```

## 2. Stored Procedures y Scripts SQL
Debido a que EF Core tiene limitaciones para manejar ciertos tipos de lógica compleja o para optimizar consultas de reporte, el proyecto utiliza Stored Procedures nativos ubicados en:
`Infrastructure/StoredProcedures/*.sql`

### Automatización de Scripts
Para evitar ejecuciones manuales, el servidor de API lee y ejecuta automáticamente todos los archivos `.sql` encontrados en esa carpeta durante el arranque (`Program.cs`):
1. Detecta archivos `.sql`.
2. Lee el contenido (scripts `CREATE OR ALTER PROCEDURE`).
3. Los ejecuta en SQL Server mediante `db.Database.ExecuteSqlRaw(sql)`.

SPs incluidos actualmente:
- `sp_ValidarDisponibilidadMedico`: Lógica de choque de horarios.
- `sp_ObtenerAgendaMedico`: Reporte de agenda diaria consolidado.

## 3. Datos de Ejemplo (Seed Data)
El proyecto incluye un mecanismo de *Seeding* para asegurar que la base de datos tenga datos funcionales inmediatamente:
- Médicos pre-cargados con especialidades asignadas.
- Horarios de consulta configurados.
- Pacientes de prueba.
Esto se gestiona a través de la configuración `OnModelCreating` en el `AppDbContext` o mediante lógica de inicialización en el startup.
