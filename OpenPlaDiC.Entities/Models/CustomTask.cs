using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class CustomTask
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public string? Folio { get; set; }

    public int Type { get; set; }

    public int Status { get; set; }

    public string? Tag { get; set; }

    public int Block { get; set; }

    public int Priority { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? ParentCustomTaskId { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal Progress { get; set; }

    public int Stage { get; set; }

    public decimal Cost { get; set; }

    public decimal Hours { get; set; }

    public string? Color { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid OwnerId { get; set; }

    public virtual ICollection<CustomTaskDep> CustomTaskDepCustomTask { get; set; } = new List<CustomTaskDep>();

    public virtual ICollection<CustomTaskDep> CustomTaskDepCustomTaskDepNavigation { get; set; } = new List<CustomTaskDep>();

    public virtual ICollection<CustomTaskUser> CustomTaskUser { get; set; } = new List<CustomTaskUser>();

    public virtual ICollection<CustomTask> InverseParentCustomTask { get; set; } = new List<CustomTask>();

    public virtual Users Owner { get; set; } = null!;

    public virtual CustomTask? ParentCustomTask { get; set; }

    public virtual Project? Project { get; set; }
}
