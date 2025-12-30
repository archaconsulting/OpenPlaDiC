using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class UserView
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ViewId { get; set; }

    public int Access { get; set; }

    public virtual Users User { get; set; } = null!;

    public virtual CustomView View { get; set; } = null!;
}
