namespace OpenPlaDiC.Core.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Folio { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }     // Almacena el Hash
        public string PasswordSalt { get; set; } // El Salt único por registro
        public bool IsActive { get; set; }
        public bool IsMaster { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        // Página de inicio específica para este Usuario (Opcional - Máxima Prioridad)
        public string? HomePageView { get; set; }
        
        // Relación con perfiles
        public virtual ICollection<UserProfile> UserProfiles { get; set; }
    }

    public class Profile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Folio { get; set; }
        public virtual ICollection<UserProfile> UserProfiles { get; set; }
        // Página de inicio específica para este Perfil (Opcional)
        public string? HomePageView { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedById { get; set; }

    }

    public class UserProfile
    {
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid ProfileId { get; set; }
        public Profile Profile { get; set; }
        // Indica cuál de sus múltiples perfiles es el principal
        public bool IsPrimary { get; set; } = false;

    }
}