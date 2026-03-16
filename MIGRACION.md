# MIGRACION.md - Estrategia de Migración GACSE

Este documento detalla el pensamiento estratégico y la planificación técnica para migrar el sistema **GACSE** de un entorno de desarrollo/prueba a un entorno de producción o para la transición desde un sistema legado.

## 1. Visión Estratégica
La migración no es solo mover datos; es asegurar la continuidad del servicio hospitalario. Nuestra estrategia se basa en:
- **Cero tiempo de inactividad (Zero Downtime):** Mediante el uso de contenedores y despliegues Blue-Green.
- **Integridad de Datos:** Validaciones rigurosas pre y post migración.
- **Escalabilidad:** Transparencia en la transición hacia infraestructuras en la nube (Azure/AWS).

## 2. Fases de la Migración

### Fase 1: Preparación y Auditoría
- **Auditoría de Datos Legados:** Identificación de inconsistencias en registros de médicos y pacientes.
- **Sanitización:** Limpieza de formatos de teléfonos y correos electrónicos.
- **Mapeo de Especialidades:** Asegurar que las especialidades del sistema anterior coincidan con nuestro enum `EspecialidadMedica`.

### Fase 2: Infraestructura y Base de Datos
- **Automatización con Migraciones:** El sistema utiliza `Entity Framework Core Migrations` para garantizar que el esquema sea idéntico en todos los entornos.
- **Despliegue de Stored Procedures:** Automatización de la carga de lógica SQL (`sp_ValidarDisponibilidadMedico`, `sp_ObtenerAgendaMedico`) al iniciar el contenedor.
- **Dockerización:** Uso de `docker-compose` para asegurar que las dependencias de red y volúmenes de datos estén aisladas y sean portables.

### Fase 3: Ejecución (Estrategia de Carga)
- **Carga Inicial (Seed Data):** Migración de catálogos maestros (Médicos y Especialidades).
- **Migración Delta:** Importación de pacientes y citas activas en una ventana de mantenimiento mínima.
- **Validación Automática:** Scripts post-carga para verificar que no existan solapamientos de citas tras la importación.

## 3. Mitigación de Riesgos y Rollback
- **Plan de Contingencia:** Si la migración falla en más del 5%, se activa el rollback inmediato al sistema anterior.
- **Backups:** Generación de snapshots de SQL Server antes de iniciar la migración delta.
- **Logs de Auditoría:** Registro detallado de cada fila migrada para rastrear discrepancias.

## 4. Próximos Pasos (V2)
- Implementación de **Event Sourcing** para sincronización en tiempo real con sistemas externos.
- Exposición de webhooks para notificaciones de cambios en la agenda durante la fase de convivencia de sistemas.
