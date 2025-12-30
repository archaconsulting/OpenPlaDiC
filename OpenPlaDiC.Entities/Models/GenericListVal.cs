using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class GenericListVal
{
    public Guid Id { get; set; }

    public Guid GenericListId { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public string? Folio { get; set; }

    public string Value { get; set; } = null!;

    public string? GroupName { get; set; }

    public string? Extra { get; set; }

    public int Type { get; set; }

    public string? Color { get; set; }

    public int Sequence { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid OwnerId { get; set; }

    public virtual GenericList GenericList { get; set; } = null!;

    public virtual Users Owner { get; set; } = null!;
}
