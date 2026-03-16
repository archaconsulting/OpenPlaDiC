using System;
using System.Collections.Generic;
using System.Text;

namespace OpenPlaDiC.SF
{
    internal class SFData
    {
        public object nextRecordsUrl { get; set; }
        public int totalSize { get; set; }
        public bool done { get; set; }
        public List<Object> records { get; set; }
    }
}

