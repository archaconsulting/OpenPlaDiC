using System;

namespace OpenPlaDiC.Core.Models;

public class EntityProperty
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string Name { get; set; }
        public string Label { get; set; }
        public int DataTypeId { get; set; }
        public string SourceDefinition { get; set; } // Tu antigua "Formula"
        
        // Configuración de Integridad
        public bool IsRequired { get; set; }
        public bool IsUnique { get; set; }
        public bool IsIndexed { get; set; }
        public bool AllowCascadeDelete { get; set; }

        // --- ESTAS SON LAS QUE DEBEN ESTAR ---
        public bool IsVisible { get; set; } 
        public bool IsEditable { get; set; }         
        public bool IsFilter { get; set; }         
        
        // Layout para Renderizado Dinámico
        public int GridRow { get; set; }
        public int GridColumn { get; set; }
        public bool OnList { get; set; }
        public int Sequence { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }

        // Propiedad de navegación de regreso a la Entidad
        public virtual Entity Entity { get; set; }
    }