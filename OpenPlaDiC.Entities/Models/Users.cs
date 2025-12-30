using System;
using System.Collections.Generic;

namespace OpenPlaDiC.Entities.Models;

public partial class Users
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public string? Folio { get; set; }

    public string? ExternalId { get; set; }

    public bool IsMaster { get; set; }

    public bool IsConfirmed { get; set; }

    public bool IsActive { get; set; }

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? MobilePhone { get; set; }

    public string? LandlinePhone { get; set; }

    public string? Avatar { get; set; }

    public string? Token { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime ModificationDate { get; set; }

    public Guid ProfileId { get; set; }

    public Guid? OwnerId { get; set; }

    public virtual ICollection<CustomTask> CustomTask { get; set; } = new List<CustomTask>();

    public virtual ICollection<CustomTaskUser> CustomTaskUser { get; set; } = new List<CustomTaskUser>();

    public virtual ICollection<CustomView> CustomView { get; set; } = new List<CustomView>();

    public virtual ICollection<GenericList> GenericList { get; set; } = new List<GenericList>();

    public virtual ICollection<GenericListVal> GenericListVal { get; set; } = new List<GenericListVal>();

    public virtual ICollection<Users> InverseOwner { get; set; } = new List<Users>();

    public virtual ICollection<Message> MessageOwner { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSender { get; set; } = new List<Message>();

    public virtual ICollection<MetadataObject> MetadataObject { get; set; } = new List<MetadataObject>();

    public virtual ICollection<ObjectList> ObjectList { get; set; } = new List<ObjectList>();

    public virtual ICollection<ObjectProp> ObjectProp { get; set; } = new List<ObjectProp>();

    public virtual Users? Owner { get; set; }

    public virtual Profile Profile { get; set; } = null!;

    public virtual ICollection<Project> Project { get; set; } = new List<Project>();

    public virtual ICollection<Report> Report { get; set; } = new List<Report>();

    public virtual ICollection<ReportParam> ReportParam { get; set; } = new List<ReportParam>();

    public virtual ICollection<SelectionList> SelectionList { get; set; } = new List<SelectionList>();

    public virtual ICollection<UserObject> UserObject { get; set; } = new List<UserObject>();

    public virtual ICollection<UserRegistry> UserRegistry { get; set; } = new List<UserRegistry>();

    public virtual ICollection<UserReport> UserReport { get; set; } = new List<UserReport>();

    public virtual ICollection<UserView> UserView { get; set; } = new List<UserView>();
}
