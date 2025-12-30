using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class MetadataObject
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public string? Folio { get; set; }

    public string Prefix { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Icon { get; set; }

    public bool UseName { get; set; }

    public string NameLabel { get; set; } = null!;

    public string NameHelp { get; set; } = null!;

    public bool IsLocked { get; set; }

    public bool IsVisible { get; set; }

    public bool IsDetail { get; set; }

    public bool IsRelated { get; set; }

    public bool ControlledAccess { get; set; }

    public bool CustomStatement { get; set; }

    public string? ListStatement { get; set; }

    public string? RelatedStatement { get; set; }

    public string? FieldsStatement { get; set; }

    public string? FilterStatement { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid OwnerId { get; set; }

    public int ListRecordsCount { get; set; }

    public int RelatedRecordsCount { get; set; }

    public string? TriggerAI { get; set; }

    public string? TriggerBI { get; set; }

    public string? TriggerAU { get; set; }

    public string? TriggerBU { get; set; }

    public string? TriggerAD { get; set; }

    public string? TriggerBD { get; set; }

    public virtual ICollection<CustomView> CustomView { get; set; } = new List<CustomView>();

    public virtual ICollection<ObjectList> ObjectList { get; set; } = new List<ObjectList>();

    public virtual ICollection<ObjectProp> ObjectPropObject { get; set; } = new List<ObjectProp>();

    public virtual ICollection<ObjectProp> ObjectPropRefObject { get; set; } = new List<ObjectProp>();

    public virtual Users Owner { get; set; } = null!;

    public virtual ICollection<ProfileObject> ProfileObject { get; set; } = new List<ProfileObject>();

    public virtual ICollection<ReportParam> ReportParam { get; set; } = new List<ReportParam>();

    public virtual ICollection<UserObject> UserObject { get; set; } = new List<UserObject>();
}
