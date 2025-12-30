using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class Profile
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public string? Folio { get; set; }

    public string? Description { get; set; }

    public bool IsAvailable { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid? OwnerId { get; set; }

    public virtual ICollection<ProfileObject> ProfileObject { get; set; } = new List<ProfileObject>();

    public virtual ICollection<ProfileView> ProfileView { get; set; } = new List<ProfileView>();

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
}
