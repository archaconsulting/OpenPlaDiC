using System;

namespace OpenPlaDiC.Core.Models;

public class SystemParameter
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsSystem { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid? UpdatedById { get; set; }
    }
