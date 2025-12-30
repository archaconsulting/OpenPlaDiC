using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPlaDiC.Framework
{
    public class GlobalItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Flag { get; set; }
        public int Code { get; set; }
        public object Tag { get; set; }

        public GlobalItem(string name, string value)
        {
            Id = Guid.Empty;
            Name = name;
            Value = value;
        }
    }
}
