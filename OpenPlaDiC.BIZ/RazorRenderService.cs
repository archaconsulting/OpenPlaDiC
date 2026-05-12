using System;
using OpenPlaDiC.Framework;

namespace OpenPlaDiC.BIZ;


public interface IRazorRenderService
{
    // Cambiamos a Response<string> para manejar errores de compilación
    Task<Response<string>> RenderToStringAsync(string viewName, object model);
}
public class RazorRenderService
{

}
