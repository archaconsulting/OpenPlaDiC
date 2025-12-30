using Microsoft.AspNetCore.Identity;

namespace OpenPlaDiC.WebApp.Models
{
    public class ApplicationRole : IdentityRole
    {
        // Puedes agregar propiedades extra si la API las devuelve
        public string Descripcion { get; set; }
    }
}
