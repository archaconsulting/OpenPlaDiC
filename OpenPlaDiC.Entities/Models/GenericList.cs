using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class GenericList
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public string? Folio { get; set; }

    public int Type { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid OwnerId { get; set; }

    public virtual ICollection<GenericListVal> GenericListVal { get; set; } = new List<GenericListVal>();

    public virtual Users Owner { get; set; } = null!;
}
