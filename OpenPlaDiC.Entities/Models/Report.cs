using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class Report
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Icon { get; set; }

    public string Query { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public bool RequiresSession { get; set; }

    public bool AvailableWithSession { get; set; }

    public bool AvailableWithoutSession { get; set; }

    public bool InMenu { get; set; }

    public string Type { get; set; } = null!;

    public bool ControlledAccess { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid OwnerId { get; set; }

    public virtual Users Owner { get; set; } = null!;

    public virtual ICollection<ReportParam> ReportParam { get; set; } = new List<ReportParam>();

    public virtual ICollection<UserReport> UserReport { get; set; } = new List<UserReport>();
}
