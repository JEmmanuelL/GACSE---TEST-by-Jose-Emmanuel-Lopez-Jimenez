# GACSE - Mini Agenda Médica (Proyecto Full Stack)

Este repositorio contiene la solución completa para el sistema de gestión de citas médicas **GACSE**.

## Estructura del Proyecto

El proyecto está dividido en dos componentes principales:

### 1. [GACSE (Backend)](./GACSE) - **Núcleo del Proyecto**
Esta es la carpeta más importante del repositorio. Contiene la **API REST** desarrollada en **C# .NET 8** siguiendo una **Arquitectura Limpia (Clean Architecture)**.

*   **¿Por qué es el componente principal?** Aquí reside toda la lógica de negocio técnica pedida: validación de conflictos de horario, manejo de duraciones por especialidad, Stored Procedures complejos, pruebas unitarias y gestión de base de datos con Entity Framework Core.
*   **Documentación:** Dentro de esta carpeta encontrarás un `README.md` detallado con instrucciones de ejecución (Docker/Local), descripción de endpoints y arquitectura.

### 2. [GACSE FRONT END (Frontend)](./GACSE%20FRONT%20END)
Contiene una interfaz de usuario sencilla desarrollada con **HTML, CSS (Bootstrap) y Vanilla JavaScript**.

*   **Propósito:** Sirve como un cliente ligero para demostrar el consumo de la API. No contiene lógica compleja, ya que su función es puramente visual y de integración para probar el flujo de la agenda desde la perspectiva del usuario.

## Requisitos Rápidos
- **Docker & Docker Compose** (para ejecutar ambos componentes fácilmente).
- **.NET 8 SDK** (para desarrollo del backend).

---
*Para instrucciones detalladas de configuración y despliegue del sistema principal, por favor diríjase al [README del Backend](./GACSE/README.md).*
