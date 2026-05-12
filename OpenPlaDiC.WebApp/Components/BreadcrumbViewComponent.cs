using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing; // Asegúrate de agregar este using



namespace OpenPlaDiC.WebApp.Components
{
    public class BreadcrumbViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var routeData = ViewContext.RouteData.Values;
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Text = "Home", Action = "Index", Controller = "Dashboard", IsActive = false }
            };

            string controller = routeData["controller"]?.ToString();
            string action = routeData["action"]?.ToString();
            string entityName = routeData["entityName"]?.ToString() ?? Request.Query["entityName"].ToString();

            // Lógica para Entidades Dinámicas
            if (controller == "Dynamic")
            {
        
                breadcrumbs.Add(new BreadcrumbItem { 
                        Text = entityName, 
                        Action = "Index", 
                        Controller = "Dynamic", 
                        RouteValues = new RouteValueDictionary { { "entityName", entityName } }, // Uso de RouteValueDictionary
                        IsActive = action == "Index" 
                    });        

                if (action == "Edit")
                {
                    breadcrumbs.Add(new BreadcrumbItem { Text = "Editor", IsActive = true });
                }
            }
            // Lógica para otras vistas del Kernel
            else if (controller != "Dashboard")
            {
                breadcrumbs.Add(new BreadcrumbItem { Text = controller, IsActive = true });
            }

            return View(breadcrumbs);
        }
    }

public class BreadcrumbItem
{
    public string Text { get; set; }
    public string Action { get; set; }
    public string Controller { get; set; }
    public object RouteValues { get; set; } // Volvemos a object
    public bool IsActive { get; set; }
}

}
