using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class CustomView
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string? Icon { get; set; }

    public Guid? ObjectId { get; set; }

    public string Content { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public bool RequiresSession { get; set; }

    public bool AvailableWithSession { get; set; }

    public bool AvailableWithoutSession { get; set; }

    public bool InMenu { get; set; }

    public string Type { get; set; } = null!;

    public bool UseAsStart { get; set; }

    public bool ControlledAccess { get; set; }

    public bool IsActive { get; set; }

    public DateTime? StartExecutionDate { get; set; }

    public DateTime? EndExecutionDate { get; set; }

    public TimeOnly? StartExecutionTime { get; set; }

    public TimeOnly? EndExecutionTime { get; set; }

    public DateTime? LastExecutionDate { get; set; }

    public DateTime? NextExecutionDate { get; set; }

    public int FrequencyMinutes { get; set; }

    public int Repetitions { get; set; }

    public bool Mon { get; set; }

    public bool Tue { get; set; }

    public bool Wed { get; set; }

    public bool Thu { get; set; }

    public bool Fri { get; set; }

    public bool Sat { get; set; }

    public bool Sun { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid OwnerId { get; set; }

    public virtual MetadataObject? Object { get; set; }

    public virtual Users Owner { get; set; } = null!;

    public virtual ICollection<ProfileView> ProfileView { get; set; } = new List<ProfileView>();

    public virtual ICollection<UserView> UserView { get; set; } = new List<UserView>();
}
