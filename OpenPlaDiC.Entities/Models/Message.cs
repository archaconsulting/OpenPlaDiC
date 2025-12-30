using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class Message
{
    public Guid Id { get; set; }

    public string Subject { get; set; } = null!;

    public int Number { get; set; }

    public string Content { get; set; } = null!;

    public bool IsNew { get; set; }

    public bool IsNotification { get; set; }

    public bool IsTextOnly { get; set; }

    public bool SendEmail { get; set; }

    public bool SendSMS { get; set; }

    public Guid? RegistryId { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? ReadDate { get; set; }

    public Guid SenderId { get; set; }

    public Guid OwnerId { get; set; }

    public virtual Users Owner { get; set; } = null!;

    public virtual Registry? Registry { get; set; }

    public virtual Users Sender { get; set; } = null!;
}
