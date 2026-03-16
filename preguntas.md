# Análisis del Proyecto GACSE - Mini Agenda Médica

Este documento contiene las respuestas a las preguntas sobre la calidad y estructura del proyecto GACSE, basadas en el examen exhaustivo del código fuente y la documentación.

## Arquitectura

**¿Separó capas correctamente?**
Sí, el proyecto implementa una **Arquitectura Limpia (Clean Architecture)** con una clara separación en cuatro capas principales:
*   **Domain:** Contiene las entidades base (`Medico`, `Paciente`, `Cita`), enums y constantes, sin dependencias externas.
*   **Application:** Define las interfaces y la lógica de negocio en servicios (`CitaService`, `MedicoService`). Utiliza DTOs para la transferencia de datos.
*   **Infrastructure:** Gestiona el acceso a datos mediante el patrón Repository y Entity Framework Core, además de contener los Stored Procedures.
*   **API (Web):** Los controladores (`Controllers`) solo actúan como puntos de entrada, delegando la lógica a la capa de aplicación.

**¿Usó patrones de diseño?**
Sí, se identificaron múltiples patrones:
*   **Repository Pattern:** Para desacoplar la lógica de negocio del acceso a datos.
*   **Dependency Injection (DI):** Utilizado extensivamente para inyectar servicios y repositorios.
*   **DTO (Data Transfer Object):** Para evitar exponer las entidades de dominio directamente en la API.
*   **Singleton/Static Constants:** Para manejar duraciones de citas de forma centralizada.

**¿El código es limpio y legible?**
Sí. El código sigue las convenciones estándar de C# (PascalCase para clases/métodos, camelCase para variables locales). No hay "código espagueti"; cada método tiene una responsabilidad única y clara. Se observa el uso de LINQ de forma eficiente y manejo adecuado de excepciones mediante un middleware global.

**¿Los nombres de variables y métodos son descriptivos?**
Altamente descriptivos. Ejemplos: `AgendarCitaAsync`, `ObtenerHorariosDisponiblesAsync`, `ConflictoHorarioResponseDTO`, `duracionMinutos`. Los nombres indican claramente la intención y el tipo de dato que manejan.

**¿El proyecto tiene estructura lógica?**
La estructura es muy lógica y profesional. Sigue el estándar industrial para aplicaciones .NET modernas, facilitando la mantenibilidad y escalabilidad.

---

## Reglas de Negocio Implementadas

**¿Valida conflictos de horario correctamente?**
Sí. La validación se realiza en dos niveles:
1.  **Capa de Aplicación:** En `CitaService.cs`, se recuperan las citas del día y se verifica mediante lógica de solapamiento de rangos (`dto.Hora < finExistente && horaFinCita > c.Hora`).
2.  **Capa de Datos:** Existe el SP `sp_ValidarDisponibilidadMedico` que realiza una validación redundante/optimizada en SQL Server.

**¿Maneja duraciones por especialidad?**
Sí. Las duraciones están centralizadas en `GACSE.Domain.Constants.DuracionCitas`, asignando tiempos específicos (v.g., Medicina General 20 min, Cirugía 45 min). El sistema usa estas constantes tanto para validar conflictos como para calcular la hora de fin de la cita.

**¿La sugerencia de horarios funciona?**
Sí. Cuando se detecta un conflicto, el sistema no solo rechaza la petición, sino que captura la excepción y retorna un objeto `ConflictoHorarioResponseDTO` con los próximos 5 horarios disponibles calculados dinámicamente.

**¿Las reglas son robustas ante casos borde?**
El sistema maneja múltiples casos borde con rigor:
*   **Citas en el pasado:** Bloqueadas mediante validación de fecha y hora.
*   **Fuera de horario:** Verifica que la cita esté dentro del horario de atención del médico.
*   **Citas que no "caben":** Si una cita de 45 min se pide 15 min antes del fin de turno del médico, el sistema la rechaza.
*   **Alerta de cancelaciones:** Implementa una regla de "alerta" (no bloqueante) si un paciente tiene 3+ cancelaciones recientes.

---

## SQL Server y Stored Procedures

**¿Los stored procedures están bien escritos?**
Sí. Utilizan `CREATE OR ALTER` para facilitar despliegues repetibles, tienen comentarios descriptivos y manejan la lógica de negocio de manera eficiente.

**¿Usa parámetros correctamente?**
Sí, utiliza parámetros tipados y parámetros de salida (`OUTPUT`) para comunicar resultados de validación de forma eficiente de vuelta al código C#.

**¿Maneja transacciones?**
Las transacciones de escritura son gestionadas de forma transparente por Entity Framework Core. Los SPs actuales están enfocados en la lectura y validación rápida de disponibilidad.

**¿La estructura de tablas tiene sentido para el dominio hospitalario?**
Sí. Las entidades `Medico`, `Paciente`, `Cita` y `HorarioMedico` están correctamente relacionadas. Especialmente importante es `HorarioMedico`, que permite definir la disponibilidad semanal de cada doctor de forma flexible.

---

## Tests y Documentación

**¿Los tests cubren las reglas críticas?**
Sí. Se identificaron 11 tests unitarios en `CitaServiceTests.cs` que cubren:
*   Conflictos de horario.
*   Validación de duraciones por especialidad (los 5 casos).
*   Validación de fechas pasadas.
*   Sugerencia de horarios disponibles.
*   Citas fuera de horario del médico.

**¿El README permite ejecutar sin preguntar?**
El `README.md` es excepcional. Incluye instrucciones paso a paso para ejecutar con **Docker Compose** (un solo comando) o de forma local con `dotnet run`. También detalla los endpoints de la API y las reglas de negocio implementadas.

**¿La respuesta de migración demuestra pensamiento estratégico?**
Sí. El proyecto incluye un mecanismo en `Program.cs` que ejecuta automáticamente `db.Database.Migrate()` y luego despliega los Stored Procedures desde archivos `.sql` al iniciar la aplicación. Esto asegura que cualquier entorno (dev, test o prod) tenga siempre la última versión de la base de datos sin intervención manual, lo cual es una estrategia de CI/CD sólida.
