using System;
using OpenPlaDiC.Core.Models;

namespace OpenPlaDiC.WebApp.Models;

public class DynamicFormViewModel
{
    public Entity EntityMetadata { get; set; }
    public Dictionary<string, object> RecordData { get; set; }
    public int AccessLevel { get; set; }
    public string? EntityName { get; set; }
    public string? ReturnUrl { get; set; } // Guardar la ruta de retorno
}