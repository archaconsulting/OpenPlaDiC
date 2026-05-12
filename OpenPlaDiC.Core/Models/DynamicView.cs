using System;

namespace OpenPlaDiC.Core.Models;

 public class DynamicView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public string Content { get; set; } // Aquí reside el código Razor
        public string ViewType { get; set; } // VIEW, ACTION, API, APIEX, TASK
        
        public int AccessLevel { get; set; }
        public bool IsActive { get; set; }
        
        // Configuración para Tareas Programadas (TASK)
        public int? FrequencyMinutes { get; set; }
        public DateTime? NextExecutionDateTime { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } 
        public Guid CreatedById { get; set; }

        public bool IsPublic { get; set; } // Nuevo flag para acceso anónimo
        
    }
