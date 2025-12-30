using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class ReportParam
{
    public Guid Id { get; set; }

    public Guid ReportId { get; set; }

    public string Name { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Help { get; set; }

    public int Type { get; set; }

    public string? DefaultValue { get; set; }

    public string? RegExp { get; set; }

    public int MaxLng { get; set; }

    public decimal MaxVal { get; set; }

    public decimal MinVal { get; set; }

    public bool IsRequired { get; set; }

    public Guid? RefObjectId { get; set; }

    public string? Formula { get; set; }

    public string GroupName { get; set; } = null!;

    public int Sequence { get; set; }

    public int ColumnPos { get; set; }

    public int RowPos { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid OwnerId { get; set; }

    public virtual Users Owner { get; set; } = null!;

    public virtual MetadataObject? RefObject { get; set; }

    public virtual Report Report { get; set; } = null!;
}
