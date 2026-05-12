using Microsoft.AspNetCore.Identity;

namespace OpenPlaDiC.WebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Propiedades adicionales de tu tabla existente
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMaster { get; set;}
    }
}
