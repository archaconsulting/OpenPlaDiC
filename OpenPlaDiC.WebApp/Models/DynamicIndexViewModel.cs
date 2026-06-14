using OpenPlaDiC.Core.Models; // Ajusta según dónde guardes la metadata de tus entidades

namespace OpenPlaDiC.WebApp.Models;

public class DynamicIndexViewModel
{
    public required dynamic EntityMetadata { get; set; } 
    public required IEnumerable<Dictionary<string, object>> Data { get; set; }
    public required Dictionary<string, string> CurrentFilters { get; set; }
    public required int CurrentPage { get; set; }
    

}