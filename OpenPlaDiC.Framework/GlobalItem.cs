using System;
using System.Collections.Generic;
using System.Text;

namespace OpenPlaDiC.Framework
{
    public class GlobalItem
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public object? Value { get; set; }
        public bool Flag { get; set; } = false;
        public int Code { get; set; } = 0;
        public object? Tag { get; set; }
        public string? Text { get; set; }

        public GlobalItem() 
        {
            Id = Guid.Empty;
        }
        public GlobalItem(string name, object value)
        {
            Id = Guid.Empty;
            Name = name;
            Value = value;
        }
    }
}
