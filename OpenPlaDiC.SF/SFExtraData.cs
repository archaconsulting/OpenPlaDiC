using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenPlaDiC.SF
{
    internal class SFExtraData
    {
        public string Name { get; set; }
        public JToken Value { get; set; }

        public SFExtraData(string name, JToken value)
        {
            Name = name;
            Value = value;
        }
    }
}
