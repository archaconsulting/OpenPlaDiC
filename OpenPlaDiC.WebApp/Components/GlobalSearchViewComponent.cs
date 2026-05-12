using System;
using Microsoft.AspNetCore.Mvc;

namespace OpenPlaDiC.WebApp.Components;

    public class GlobalSearchViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            // Aquí podrías cargar configuraciones si la búsqueda tuviera filtros
            return View();
        }
    }