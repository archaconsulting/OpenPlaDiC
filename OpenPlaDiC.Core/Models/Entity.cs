using System;

namespace OpenPlaDiC.Core.Models;

public class Entity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public bool IsSystem { get; set; }
        public bool UseNameField { get; set; }
        public string NameLabel { get; set; }
        public string NameHelpText { get; set; }    // Texto de ayuda para el input Name

        
        // Triggers Razor
        public string? OnBeforeInsert { get; set; }
        public string? OnAfterInsert { get; set; }
        public string? OnBeforeUpdate { get; set; }
        public string? OnAfterUpdate { get; set; }
        public string? OnBeforeDelete { get; set; }
        public string? OnAfterDelete { get; set; }

        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
        public string? ListQuery{ get; set; }
        public int PageSize { get; set; }
        
        // Relación con propiedades
        public virtual ICollection<EntityProperty>? Properties { get; set; }
    }
