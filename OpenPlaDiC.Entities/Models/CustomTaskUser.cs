using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class CustomTaskUser
{
    public Guid Id { get; set; }

    public bool IsExternal { get; set; }

    public bool IsActive { get; set; }

    public bool CanModifyCustomTask { get; set; }

    public bool CanPublishLog { get; set; }

    public decimal Cost { get; set; }

    public int Weight { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public Guid CustomTaskId { get; set; }

    public Guid? UserId { get; set; }

    public string? Email { get; set; }

    public string? MobilePhone { get; set; }

    public virtual CustomTask CustomTask { get; set; } = null!;

    public virtual Users? User { get; set; }
}
