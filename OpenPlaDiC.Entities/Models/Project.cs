using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class Project
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public string? Folio { get; set; }

    public int Status { get; set; }

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal Progress { get; set; }

    public decimal Cost { get; set; }

    public string? Color { get; set; }

    public string? Image { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid OwnerId { get; set; }

    public virtual ICollection<CustomTask> CustomTask { get; set; } = new List<CustomTask>();

    public virtual Users Owner { get; set; } = null!;
}
