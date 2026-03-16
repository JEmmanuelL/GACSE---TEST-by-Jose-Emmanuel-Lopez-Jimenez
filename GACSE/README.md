# GACSE - Mini Agenda Médica API

API REST en C# .NET 8 para gestionar citas médicas en un hospital. Desarrollada como prueba técnica demostrando arquitectura por capas, Entity Framework Core, SQL Server, Docker y buenas prácticas de desarrollo backend.

## Tecnologías

- **C# .NET 8** Web API
- **SQL Server** (vía Docker)
- **Entity Framework Core** + Stored Procedures
- **Docker Compose**
- **Swagger / OpenAPI**
- **xUnit + Moq** (Unit Tests)

## Cómo ejecutar

### Con Docker Compose (recomendado)

```bash
docker compose up --build
```

La API estará disponible en: **http://localhost:8080/swagger**

### Sin Docker (desarrollo local)

Requisitos: .NET 8 SDK, SQL Server local.

1. Actualizar connection string en `appsettings.json`
2. Ejecutar:
```bash
dotnet ef database update
dotnet run
```

### Ejecutar tests

```bash
dotnet test
```

## Endpoints

### Médicos (`/api/medicos`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/medicos` | Listar todos los médicos |
| GET | `/api/medicos/{id}` | Obtener médico por Id |
| POST | `/api/medicos` | Crear médico |
| PUT | `/api/medicos/{id}` | Actualizar médico |
| DELETE | `/api/medicos/{id}` | Eliminar médico |
| GET | `/api/medicos/{id}/horarios-disponibles?fecha=YYYY-MM-DD` | Próximos 5 horarios disponibles |

### Pacientes (`/api/pacientes`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/pacientes` | Listar todos los pacientes |
| GET | `/api/pacientes/{id}` | Obtener paciente por Id |
| POST | `/api/pacientes` | Crear paciente |
| PUT | `/api/pacientes/{id}` | Actualizar paciente |
| DELETE | `/api/pacientes/{id}` | Eliminar paciente |
| GET | `/api/pacientes/{id}/historial` | Historial de citas del paciente |

### Citas (`/api/citas`)
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/citas` | Agendar cita |
| PUT | `/api/citas/{id}/cancelar` | Cancelar cita |
| GET | `/api/citas` | Consultar todas las citas |
| GET | `/api/citas/agenda?medicoId=X&fecha=YYYY-MM-DD` | Agenda del día de un médico |

## Reglas de Negocio

1. **Sin citas simultáneas** — Un médico no puede tener dos citas al mismo tiempo
2. **Duración por especialidad** — Medicina General: 20 min, Cardiología: 30 min, Cirugía: 45 min, Pediatría: 20 min, Ginecología: 30 min
3. **Horario del médico** — Las citas deben estar dentro del horario de consulta
4. **Sin citas en el pasado** — No se pueden agendar citas en fechas/horas que ya pasaron
5. **Alerta de cancelaciones** — Si un paciente tiene 3+ cancelaciones en los últimos 30 días, se incluye una alerta (no bloquea)
6. **Sugerencia de horarios** — Si hay conflicto, se devuelven los próximos 5 horarios disponibles

## Stored Procedures

- `sp_ValidarDisponibilidadMedico` — Valida disponibilidad del médico para una fecha y hora
- `sp_ObtenerAgendaMedico` — Obtiene la agenda completa del día de un médico

## Estructura del Proyecto

```
GACSE/
├── Controllers/          → Endpoints REST (sin lógica de negocio)
├── Domain/
│   ├── Entities/         → Medico, Paciente, Cita, HorarioMedico
│   ├── Enums/            → EspecialidadMedica, EstadoCita
│   └── Constants/        → DuracionCitas
├── Application/
│   ├── DTOs/             → Data Transfer Objects
│   ├── Interfaces/       → Contratos de Services y Repositories
│   └── Services/         → Lógica de negocio
├── Infrastructure/
│   ├── Data/             → AppDbContext + Migraciones EF Core
│   ├── Repositories/     → Acceso a datos
│   └── StoredProcedures/ → Scripts SQL
├── Middlewares/           → Manejo global de excepciones
├── Extensions/            → Configuración de DI
├── Tests/                 → Unit Tests (xUnit + Moq)
├── docker-compose.yml
├── Dockerfile
└── Program.cs
```

## Arquitectura

```
Controller → Service (Business Logic) → Repository → EF Core / Stored Procedure → SQL Server
```

Los Controllers solo reciben requests y devuelven responses. Toda la lógica de negocio reside en la capa de Services. Los Repositories encapsulan el acceso a datos.

## Códigos HTTP

| Código | Uso |
|--------|-----|
| 200 OK | Operación exitosa |
| 201 Created | Recurso creado |
| 204 No Content | Eliminación exitosa |
| 400 Bad Request | Datos inválidos, cita en el pasado, fuera de horario |
| 404 Not Found | Recurso no encontrado |
| 409 Conflict | Conflicto de horario |

## Unit Tests

11 tests cubriendo las reglas críticas:
- Conflicto de horarios
- Duración por especialidad (5 variantes)
- Validación de citas en el pasado
- Alerta de cancelaciones
- Sugerencia de horarios
- Cita fuera de horario del médico
- Cita que no cabe antes del cierre

## Datos de Ejemplo (Seed Data)

La base de datos se inicializa automáticamente con:
- 4 médicos de diferentes especialidades
- 6 pacientes
- 15 horarios de consulta
- 5 citas de ejemplo
