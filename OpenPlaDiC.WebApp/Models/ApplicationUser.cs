using Microsoft.AspNetCore.Identity;

namespace OpenPlaDiC.WebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Propiedades adicionales de tu tabla existente
        public string NombreCompleto { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
