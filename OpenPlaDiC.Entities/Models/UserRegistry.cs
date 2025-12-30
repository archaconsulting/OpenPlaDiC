using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class UserRegistry
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RegistryId { get; set; }

    public int Access { get; set; }

    public bool DenyAccess { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public virtual Registry Registry { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
