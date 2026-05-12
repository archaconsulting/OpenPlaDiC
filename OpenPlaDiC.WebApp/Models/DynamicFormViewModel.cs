using System;
using OpenPlaDiC.Core.Models;

namespace OpenPlaDiC.WebApp.Models;

public class DynamicFormViewModel
{
    public Entity EntityMetadata { get; set; }
    public Dictionary<string, object> RecordData { get; set; }
    public int AccessLevel { get; set; }
}