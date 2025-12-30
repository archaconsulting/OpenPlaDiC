using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class Registry
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string Folio { get; set; } = null!;

    public string? Object { get; set; }

    public string? Extra { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public virtual ICollection<Message> Message { get; set; } = new List<Message>();

    public virtual ICollection<UserRegistry> UserRegistry { get; set; } = new List<UserRegistry>();
}
