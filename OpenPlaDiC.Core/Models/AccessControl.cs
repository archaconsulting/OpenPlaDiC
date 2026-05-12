using System;

namespace OpenPlaDiC.Core.Models;

    public class AccessControl
    {
        public Guid Id { get; set; }
        public Guid? ProfileId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? EntityId { get; set; }
        public Guid? DynamicViewId { get; set; }
        
        public int AccessLevel { get; set; }
        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExecute { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }
    }
