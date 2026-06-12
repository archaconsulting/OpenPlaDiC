# 🚀 OpenPlaDiC (Open Platform for Dynamic Components)

[![License: Apache-2.0](https://shields.io)](https://opensource.org)
[![.NET Core](https://shields.io)](https://microsoft.com)
[![Architecture: Clean](https://shields.io)]()

**OpenPlaDiC** es una plataforma web *Low-Code* y un motor relacional dinámico basado en .NET 8, diseñado para permitir la creación, gestión y automatización de estructuras de datos (entidades y campos) en tiempo de ejecución desde el navegador, sin necesidad de escribir código SQL o recompilar la aplicación.

Ideal para construir CRMs, ERPs, sistemas de gestión a la medida o cualquier aplicación que requiera una alta flexibilidad en su modelo de datos y flujos transaccionales dinámicos.

---

## 💎 Características Principales

*   **Diseñador de Entidades Dinámico (Entity Manager):** CRUD web para crear tablas físicas en SQL Server y sus respectivas propiedades metadata al instante con control de borrado e inyección segura.
*   **Distribución de UI Inteligente:** Sistema nativo de renderizado asimétrico estandarizado en una cuadrícula equilibrada de 2 columnas de ancho de Bootstrap 5 (Lado Izquierdo / Lado Derecho).
*   **Vistas Maestro-Detalle Dinámicas:** Auto-descubrimiento de relaciones e integración automática de sub-tarjetas apiladas verticalmente para módulos vinculados en la edición de registros.
*   **Mecanismo de Lookups y Autocompletado:** Buscador relacional de llaves foráneas con soporte para modales genéricos y sugerencias fluidas en tiempo real (`quickSearch`).
*   **Triggers de Negocio con Razor:** Ejecución de código C# dinámico guardado en base de datos e interactuando antes o después de persistir datos (`OnBeforeInsert`, `OnAfterUpdate`, etc.).
*   **Pista de Auditoría Avanzada (Audit Trail):** Registro en formato JSON de valores modificados (Deltas), permitiendo comparaciones visuales de "Antes vs Después" para supervisores.
*   **Kernel de Seguridad Basado en Claims:** Bypass inteligente de validación de accesos para súper usuarios administradores (`IsMaster`).
*   **Email Engine Integrado:** Servicio robusto de envío y recepción IMAP/SMTP con soporte profesional para procesamiento de archivos adjuntos.
*   **Caché Reactiva del Kernel:** Invalidez de caché en memoria de parámetros críticos del sistema sin requerir reinicios del servidor IIS/Kestrel.

---

## 🏗️ Arquitectura del Proyecto

El sistema está desarrollado siguiendo los principios de **Clean Architecture** y separación estricta de responsabilidades en las siguientes capas:

```text
OpenPlaDiC/
├── OpenPlaDiC.Core/         # Modelos de dominio base (Entity, Property, User)
├── OpenPlaDiC.DAL/          # Contexto de base de datos (AppDbContext) y ADO.NET genérico
├── OpenPlaDiC.BIZ/          # Capa de lógica de negocio (DynamicData, Email, AccessService)
└── OpenPlaDiC.WebApp/       # Interfaz de presentación (MVC Controllers, Views, ViewComponents)
```

---

## 🛠️ Requisitos de Instalación

Para ejecutar este proyecto en tu entorno local necesitas contar con:

*   [.NET 8.0 SDK](https://microsoft.com/dotnet/8.0) o superior.
*   [SQL Server](https://microsoft.com) (Express, Developer o superior).
*   Visual Studio 2022 o Visual Studio Code.

---

## 🚀 Configuración y Despliegue Rápido

### 1. Clonar el repositorio
```bash
git clone https://github.com
cd OpenPlaDiC
```

### 2. Base de Datos
Ejecuta los scripts de esquemas estructurales ubicados en la carpeta `/Database`. Asegúrate de compilar los stored procedures del núcleo requeridos por el gestor de metadatos:
*   `sp_Core_CreateEntity` (Genera tablas físicas con columnas base del Kernel como `IsDeleted`, `CreatedAt`, etc.)
*   `sp_Core_AddProperty` (Efectúa el `ALTER TABLE` e inyecta llaves foráneas automáticas para Tipo 10)
*   `sp_Core_DropProperty` (Borrado seguro y atómico de columnas y metadatos)
*   `sp_Core_UpdatePropertyMetadata` (Permite renombrar etiquetas del sistema como el campo base `Name` a 'Razón Social' o 'Descripción')

### 3. Configuración de Credenciales
Actualiza la cadena de conexión en el archivo `OpenPlaDiC.WebApp/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=OpenPlaDiC;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Compilar y Ejecutar
Desde la consola de comandos en la raíz de la aplicación web ejecute:
```bash
dotnet clean
dotnet build
dotnet run --project OpenPlaDiC.WebApp
```
Abre tu navegador e ingresa a `http://localhost:5000` o la URL provista por Kestrel.

---

## 🤝 Contribuciones y Desarrollo

¡OpenPlaDiC es un proyecto Open Source y todas las contribuciones son bienvenidas! 

Si deseas colaborar con el código, optimizaciones de consultas dinámicas o nuevas UI adaptativas:
1. Haz un **Fork** del proyecto.
2. Crea una nueva rama para tu funcionalidad (`git checkout -b feature/AmazingFeature`).
3. Realiza tus cambios y haz un commit claro (`git commit -m 'Add some AmazingFeature'`).
4. Sube la rama (`git push origin feature/AmazingFeature`).
5. Abre un **Pull Request** detallando tus cambios.

---

## 📄 Licencia

Este proyecto está bajo la Licencia **Apache 2.0** - consulta el archivo [LICENSE](LICENSE) para obtener más detalles.
