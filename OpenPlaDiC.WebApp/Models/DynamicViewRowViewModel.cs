using System;
using OpenPlaDiC.Core.Models;

namespace OpenPlaDiC.WebApp.Models;

// ⚡ CLASE DE TRANSPORTE EXPLICITA (ViewModel)
// Colócala aquí para blindar el paso de datos a la vista
public class DynamicViewRowViewModel
{
    public DynamicView Data { get; set; } = null!;
    public bool FileExists { get; set; }
}

