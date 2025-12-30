using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class CustomTaskDep
{
    public Guid Id { get; set; }

    public Guid CustomTaskId { get; set; }

    public Guid CustomTaskDepId { get; set; }

    public virtual CustomTask CustomTask { get; set; } = null!;

    public virtual CustomTask CustomTaskDepNavigation { get; set; } = null!;
}
