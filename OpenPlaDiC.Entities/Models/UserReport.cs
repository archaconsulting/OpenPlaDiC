using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class UserReport
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ReportId { get; set; }

    public int Access { get; set; }

    public virtual Report Report { get; set; } = null!;

    public virtual Users User { get; set; } = null!;
}
