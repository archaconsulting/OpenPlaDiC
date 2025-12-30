using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class ProfileView
{
    public Guid Id { get; set; }

    public Guid ProfileId { get; set; }

    public Guid ViewId { get; set; }

    public int Access { get; set; }

    public virtual Profile Profile { get; set; } = null!;

    public virtual CustomView View { get; set; } = null!;
}
