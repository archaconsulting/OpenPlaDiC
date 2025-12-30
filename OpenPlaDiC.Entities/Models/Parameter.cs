using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class Parameter
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Value { get; set; }

    public string? GroupName { get; set; }

    public string? Extra { get; set; }

    public int Type { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }
}
