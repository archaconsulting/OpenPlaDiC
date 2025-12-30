using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPlaDiC.Framework
{
    public class Request
    {
        public string? Reference { get; set; }
        public string? View { get; set; }
        public string? Command { get; set; }
        public string? Data { get; set; }
        public string? Uid { get; set; }
        public string? Recid { get; set; }
        public List<GlobalItem>? Parameters { get; set; }
        public int? Type { get; set; }
        public string? Token { get; set; }
        public string? ApiKey { get; set; }


    }
}
