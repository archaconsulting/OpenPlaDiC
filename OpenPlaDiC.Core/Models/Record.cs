using System;

namespace OpenPlaDiC.Core.Models;

    public class Record
    {
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
        public string Folio { get; set; }
        public string SearchContent { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }

        // Propiedad de navegación para saber a qué tabla pertenece
        public virtual Entity Entity { get; set; }
    }