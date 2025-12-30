using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class Event
{
    public Guid Id { get; set; }

    public int Number { get; set; }

    public string? Folio { get; set; }

    public string Type { get; set; } = null!;

    public string? ProcedureName { get; set; }

    public string? Tag { get; set; }

    public string? SessionInfo { get; set; }

    public string? EventInfo { get; set; }

    public DateTime DateTime { get; set; }

    public string? Reference { get; set; }
}
