using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class ObjectProp
{
    public Guid Id { get; set; }

    public Guid ObjectId { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public string? Folio { get; set; }

    public string Label { get; set; } = null!;

    public string? Help { get; set; }

    public int Type { get; set; }

    public string? DefaultValue { get; set; }

    public string? RegExp { get; set; }

    public int MaxLng { get; set; }

    public decimal MaxVal { get; set; }

    public decimal MinVal { get; set; }

    public bool IsRequired { get; set; }

    public bool IsUnique { get; set; }

    public Guid? RefObjectId { get; set; }

    public string? RelatedStatement { get; set; }

    public string? FieldsStatement { get; set; }

    public string? Formula { get; set; }

    public string GroupName { get; set; } = null!;

    public int Sequence { get; set; }

    public int ColumnPos { get; set; }

    public int RowPos { get; set; }

    public int OptionVal { get; set; }

    public bool IsVisible { get; set; }

    public bool IsEditable { get; set; }

    public bool InList { get; set; }

    public bool InRelated { get; set; }

    public bool ApplyFilter { get; set; }

    public bool GenerateAccess { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid OwnerId { get; set; }

    public virtual MetadataObject Object { get; set; } = null!;

    public virtual Users Owner { get; set; } = null!;

    public virtual MetadataObject? RefObject { get; set; }

    public virtual ICollection<SelectionList> SelectionList { get; set; } = new List<SelectionList>();
}
