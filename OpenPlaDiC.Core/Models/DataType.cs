using System;

namespace OpenPlaDiC.Core.Models;

    public class DataType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SqlDefinition { get; set; }

        // Relación opcional: Una definición de tipo puede estar en muchas propiedades
        public virtual ICollection<EntityProperty> EntityProperties { get; set; }
    }