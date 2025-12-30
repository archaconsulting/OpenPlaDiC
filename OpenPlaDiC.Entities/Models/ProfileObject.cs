using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class ProfileObject
{
    public Guid Id { get; set; }

    public Guid ProfileId { get; set; }

    public Guid ObjectId { get; set; }

    public int Access { get; set; }

    public bool InMain { get; set; }

    public virtual MetadataObject Object { get; set; } = null!;

    public virtual Profile Profile { get; set; } = null!;
}
