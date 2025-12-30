using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OpenPlaDiC.Entities.Models;

namespace OpenPlaDiC.DAL;

public partial class AppDbContext : DbContext
{
    private readonly string _connectionString;

    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);
    }

   
    public virtual DbSet<CustomTask> CustomTask { get; set; }

    public virtual DbSet<CustomTaskDep> CustomTaskDep { get; set; }

    public virtual DbSet<CustomTaskUser> CustomTaskUser { get; set; }

    public virtual DbSet<CustomView> CustomView { get; set; }

    public virtual DbSet<Event> Event { get; set; }

    public virtual DbSet<GenericList> GenericList { get; set; }

    public virtual DbSet<GenericListVal> GenericListVal { get; set; }

    public virtual DbSet<Message> Message { get; set; }

    public virtual DbSet<MetadataObject> MetadataObject { get; set; }

    public virtual DbSet<ObjectList> ObjectList { get; set; }

    public virtual DbSet<ObjectProp> ObjectProp { get; set; }

    public virtual DbSet<Parameter> Parameter { get; set; }

    public virtual DbSet<Profile> Profile { get; set; }

    public virtual DbSet<ProfileObject> ProfileObject { get; set; }

    public virtual DbSet<ProfileView> ProfileView { get; set; }

    public virtual DbSet<Project> Project { get; set; }

    public virtual DbSet<Registry> Registry { get; set; }

    public virtual DbSet<Report> Report { get; set; }

    public virtual DbSet<ReportParam> ReportParam { get; set; }

    public virtual DbSet<SelectionList> SelectionList { get; set; }

    public virtual DbSet<TempData> TempData { get; set; }

    public virtual DbSet<UserObject> UserObject { get; set; }

    public virtual DbSet<UserRegistry> UserRegistry { get; set; }

    public virtual DbSet<UserReport> UserReport { get; set; }

    public virtual DbSet<UserView> UserView { get; set; }

    public virtual DbSet<Users> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CustomTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CustomTa__3214EC07DC9AC941");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("T_AI");
                    tb.HasTrigger("T_AU");
                });

            entity.HasIndex(e => new { e.ProjectId, e.ParentCustomTaskId, e.Name }, "UQ__CustomTa__1B75C244CC78CB14").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Color)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasDefaultValue("#ffffff");
            entity.Property(e => e.Cost).HasColumnType("numeric(15, 4)");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasComputedColumnSql("('T-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.Hours).HasColumnType("numeric(15, 4)");
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.Progress).HasColumnType("numeric(7, 4)");
            entity.Property(e => e.Tag)
                .HasMaxLength(120)
                .IsUnicode(false);

            entity.HasOne(d => d.Owner).WithMany(p => p.CustomTask)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomTas__Owner__6FE99F9F");

            entity.HasOne(d => d.ParentCustomTask).WithMany(p => p.InverseParentCustomTask)
                .HasForeignKey(d => d.ParentCustomTaskId)
                .HasConstraintName("FK__CustomTas__Paren__68487DD7");

            entity.HasOne(d => d.Project).WithMany(p => p.CustomTask)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK__CustomTas__Proje__6754599E");
        });

        modelBuilder.Entity<CustomTaskDep>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CustomTa__3214EC07C102CE48");

            entity.HasIndex(e => new { e.CustomTaskId, e.CustomTaskDepId }, "UQ__CustomTa__CD3240DB1D5D7D14").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.CustomTaskDepNavigation).WithMany(p => p.CustomTaskDepCustomTaskDepNavigation)
                .HasForeignKey(d => d.CustomTaskDepId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomTas__Custo__02FC7413");

            entity.HasOne(d => d.CustomTask).WithMany(p => p.CustomTaskDepCustomTask)
                .HasForeignKey(d => d.CustomTaskId)
                .HasConstraintName("FK__CustomTas__Custo__02084FDA");
        });

        modelBuilder.Entity<CustomTaskUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CustomTa__3214EC0787A2EF11");

            entity.HasIndex(e => new { e.CustomTaskId, e.UserId }, "UQ__CustomTa__C90F02299BF37120").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CanPublishLog).HasDefaultValue(true);
            entity.Property(e => e.Cost).HasColumnType("numeric(15, 4)");
            entity.Property(e => e.Email)
                .HasMaxLength(240)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MobilePhone)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.CustomTask).WithMany(p => p.CustomTaskUser)
                .HasForeignKey(d => d.CustomTaskId)
                .HasConstraintName("FK__CustomTas__Custo__7C4F7684");

            entity.HasOne(d => d.User).WithMany(p => p.CustomTaskUser)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__CustomTas__UserI__7D439ABD");
        });

        modelBuilder.Entity<CustomView>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CustomVi__3214EC07CB2BD4C2");

            entity.HasIndex(e => e.Name, "UQ__CustomVi__737584F641DFC4CC").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvailableWithSession).HasDefaultValue(true);
            entity.Property(e => e.AvailableWithoutSession).HasDefaultValue(true);
            entity.Property(e => e.Content).IsUnicode(false);
            entity.Property(e => e.ControlledAccess).HasDefaultValue(true);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndExecutionDate).HasColumnType("datetime");
            entity.Property(e => e.Fri).HasDefaultValue(true);
            entity.Property(e => e.Icon)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Label)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.LastExecutionDate).HasColumnType("datetime");
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Mon).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.NextExecutionDate).HasColumnType("datetime");
            entity.Property(e => e.RequiresSession).HasDefaultValue(true);
            entity.Property(e => e.Sat).HasDefaultValue(true);
            entity.Property(e => e.StartExecutionDate).HasColumnType("datetime");
            entity.Property(e => e.Sun).HasDefaultValue(true);
            entity.Property(e => e.Thu).HasDefaultValue(true);
            entity.Property(e => e.Tue).HasDefaultValue(true);
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("VIEW");
            entity.Property(e => e.Wed).HasDefaultValue(true);

            entity.HasOne(d => d.Object).WithMany(p => p.CustomView)
                .HasForeignKey(d => d.ObjectId)
                .HasConstraintName("FK__CustomVie__Objec__57DD0BE4");

            entity.HasOne(d => d.Owner).WithMany(p => p.CustomView)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CustomVie__Owner__6BE40491");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Event__3214EC07287042F3");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.DateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EventInfo).IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasComputedColumnSql("('EV-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.ProcedureName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Reference)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.SessionInfo).IsUnicode(false);
            entity.Property(e => e.Tag)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(120)
                .IsUnicode(false);
        });

        modelBuilder.Entity<GenericList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GenericL__3214EC07D45DCCB6");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Folio)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasComputedColumnSql("('LG-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Owner).WithMany(p => p.GenericList)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GenericLi__Owner__46B27FE2");
        });

        modelBuilder.Entity<GenericListVal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GenericL__3214EC07CBD5BAB3");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Extra)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasComputedColumnSql("('LV-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.GroupName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.Value).IsUnicode(false);

            entity.HasOne(d => d.GenericList).WithMany(p => p.GenericListVal)
                .HasForeignKey(d => d.GenericListId)
                .HasConstraintName("FK__GenericLi__Gener__4A8310C6");

            entity.HasOne(d => d.Owner).WithMany(p => p.GenericListVal)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GenericLi__Owner__503BEA1C");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Message__3214EC07A34E5C4E");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Content).IsUnicode(false);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsNew).HasDefaultValue(true);
            entity.Property(e => e.IsNotification).HasDefaultValue(true);
            entity.Property(e => e.IsTextOnly).HasDefaultValue(true);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.ReadDate).HasColumnType("datetime");
            entity.Property(e => e.Subject)
                .HasMaxLength(120)
                .IsUnicode(false);

            entity.HasOne(d => d.Owner).WithMany(p => p.MessageOwner)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__OwnerId__52593CB8");

            entity.HasOne(d => d.Registry).WithMany(p => p.Message)
                .HasForeignKey(d => d.RegistryId)
                .HasConstraintName("FK__Message__Registr__4F7CD00D");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSender)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Message__SenderI__5165187F");
        });

        modelBuilder.Entity<MetadataObject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Metadata__3214EC07F43B36BE");

            entity.ToTable(tb => tb.HasTrigger("O_AI"));

            entity.HasIndex(e => e.Prefix, "UQ__Metadata__1FB4799DB9664624").IsUnique();

            entity.HasIndex(e => e.Name, "UQ__Metadata__737584F6DF0BFA85").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ControlledAccess).HasDefaultValue(true);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FieldsStatement).IsUnicode(false);
            entity.Property(e => e.FilterStatement).IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasComputedColumnSql("('O-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.Icon)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.IsRelated).HasDefaultValue(true);
            entity.Property(e => e.IsVisible).HasDefaultValue(true);
            entity.Property(e => e.Label)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.ListRecordsCount).HasDefaultValue(-1);
            entity.Property(e => e.ListStatement).IsUnicode(false);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.NameHelp)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasDefaultValue("Name assigned to the new record");
            entity.Property(e => e.NameLabel)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasDefaultValue("Name");
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.Prefix)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.RelatedRecordsCount).HasDefaultValue(-1);
            entity.Property(e => e.RelatedStatement).IsUnicode(false);
            entity.Property(e => e.TriggerAD).IsUnicode(false);
            entity.Property(e => e.TriggerAI).IsUnicode(false);
            entity.Property(e => e.TriggerAU).IsUnicode(false);
            entity.Property(e => e.TriggerBD).IsUnicode(false);
            entity.Property(e => e.TriggerBI).IsUnicode(false);
            entity.Property(e => e.TriggerBU).IsUnicode(false);
            entity.Property(e => e.UseName).HasDefaultValue(true);

            entity.HasOne(d => d.Owner).WithMany(p => p.MetadataObject)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MetadataO__Owner__1332DBDC");
        });

        modelBuilder.Entity<ObjectList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ObjectLi__3214EC07A9034A9F");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Extra)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasComputedColumnSql("('OL-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.GroupName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.Value).IsUnicode(false);

            entity.HasOne(d => d.Object).WithMany(p => p.ObjectList)
                .HasForeignKey(d => d.ObjectId)
                .HasConstraintName("FK__ObjectLis__Objec__31B762FC");

            entity.HasOne(d => d.Owner).WithMany(p => p.ObjectList)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ObjectLis__Owner__3587F3E0");
        });

        modelBuilder.Entity<ObjectProp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ObjectPr__3214EC07368553AF");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("OP_AD");
                    tb.HasTrigger("OP_AI");
                });

            entity.HasIndex(e => new { e.ObjectId, e.Name }, "UQ__ObjectPr__FD56CADF17EED6B3").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DefaultValue)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.FieldsStatement).IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasComputedColumnSql("('OP-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.Formula).IsUnicode(false);
            entity.Property(e => e.GroupName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Help)
                .HasMaxLength(480)
                .IsUnicode(false);
            entity.Property(e => e.IsEditable).HasDefaultValue(true);
            entity.Property(e => e.IsVisible).HasDefaultValue(true);
            entity.Property(e => e.Label)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.MaxVal).HasColumnType("numeric(15, 4)");
            entity.Property(e => e.MinVal).HasColumnType("numeric(15, 4)");
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.RegExp)
                .HasMaxLength(480)
                .IsUnicode(false);
            entity.Property(e => e.RelatedStatement).IsUnicode(false);

            entity.HasOne(d => d.Object).WithMany(p => p.ObjectPropObject)
                .HasForeignKey(d => d.ObjectId)
                .HasConstraintName("FK__ObjectPro__Objec__1AD3FDA4");

            entity.HasOne(d => d.Owner).WithMany(p => p.ObjectProp)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ObjectPro__Owner__2DE6D218");

            entity.HasOne(d => d.RefObject).WithMany(p => p.ObjectPropRefObject)
                .HasForeignKey(d => d.RefObjectId)
                .HasConstraintName("FK__ObjectPro__RefOb__2180FB33");
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Paramete__3214EC07396DDE6E");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Extra)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.GroupName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Label)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Value).IsUnicode(false);
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Profile__3214EC07994C2CFB");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("P_AI");
                    tb.HasTrigger("P_AU");
                });

            entity.HasIndex(e => e.Name, "UQ__Profile__737584F6E1F3D334").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasComputedColumnSql("('P-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<ProfileObject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProfileO__3214EC0777D6F52F");

            entity.HasIndex(e => new { e.ProfileId, e.ObjectId }, "UQ__ProfileO__20AA91CC8C6A2C9E").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Access).HasDefaultValue(1);

            entity.HasOne(d => d.Object).WithMany(p => p.ProfileObject)
                .HasForeignKey(d => d.ObjectId)
                .HasConstraintName("FK__ProfileOb__Objec__01D345B0");

            entity.HasOne(d => d.Profile).WithMany(p => p.ProfileObject)
                .HasForeignKey(d => d.ProfileId)
                .HasConstraintName("FK__ProfileOb__Profi__00DF2177");
        });

        modelBuilder.Entity<ProfileView>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProfileV__3214EC07EA9A4EB1");

            entity.HasIndex(e => new { e.ProfileId, e.ViewId }, "UQ__ProfileV__58EFF92AF708AF05").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Access).HasDefaultValue(1);

            entity.HasOne(d => d.Profile).WithMany(p => p.ProfileView)
                .HasForeignKey(d => d.ProfileId)
                .HasConstraintName("FK__ProfileVi__Profi__0880433F");

            entity.HasOne(d => d.View).WithMany(p => p.ProfileView)
                .HasForeignKey(d => d.ViewId)
                .HasConstraintName("FK__ProfileVi__ViewI__09746778");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Project__3214EC07BE30411E");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("PY_AI");
                    tb.HasTrigger("PY_AU");
                });

            entity.HasIndex(e => e.Name, "UQ__Project__737584F6CE46B1A0").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Color)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasDefaultValue("#ffffff");
            entity.Property(e => e.Cost).HasColumnType("numeric(15, 4)");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasComputedColumnSql("('PY-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.Image).IsUnicode(false);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.Progress).HasColumnType("numeric(7, 4)");

            entity.HasOne(d => d.Owner).WithMany(p => p.Project)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Project__OwnerId__5CD6CB2B");
        });

        modelBuilder.Entity<Registry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Registry__3214EC07109A7B65");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Extra)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Object)
                .HasMaxLength(60)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Report__3214EC07D4C7ECE6");

            entity.HasIndex(e => e.Name, "UQ__Report__737584F647DE4350").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AvailableWithSession).HasDefaultValue(true);
            entity.Property(e => e.AvailableWithoutSession).HasDefaultValue(true);
            entity.Property(e => e.ControlledAccess).HasDefaultValue(true);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Icon)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.Label)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Query).IsUnicode(false);
            entity.Property(e => e.RequiresSession).HasDefaultValue(true);
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("LISTING");

            entity.HasOne(d => d.Owner).WithMany(p => p.Report)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Report__OwnerId__17C286CF");
        });

        modelBuilder.Entity<ReportParam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReportPa__3214EC07FF81322D");

            entity.HasIndex(e => new { e.ReportId, e.Name }, "UQ__ReportPa__B28A104B3DA7AD2A").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DefaultValue)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Formula).IsUnicode(false);
            entity.Property(e => e.GroupName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Help)
                .HasMaxLength(480)
                .IsUnicode(false);
            entity.Property(e => e.Label)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.MaxVal).HasColumnType("numeric(15, 4)");
            entity.Property(e => e.MinVal).HasColumnType("numeric(15, 4)");
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.RegExp)
                .HasMaxLength(480)
                .IsUnicode(false);

            entity.HasOne(d => d.Owner).WithMany(p => p.ReportParam)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReportPar__Owner__2EA5EC27");

            entity.HasOne(d => d.RefObject).WithMany(p => p.ReportParam)
                .HasForeignKey(d => d.RefObjectId)
                .HasConstraintName("FK__ReportPar__RefOb__28ED12D1");

            entity.HasOne(d => d.Report).WithMany(p => p.ReportParam)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK__ReportPar__Repor__2334397B");
        });

        modelBuilder.Entity<SelectionList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Selectio__3214EC07C994741A");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Extra)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasComputedColumnSql("('LS-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.GroupName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.Value).IsUnicode(false);

            entity.HasOne(d => d.ObjectProp).WithMany(p => p.SelectionList)
                .HasForeignKey(d => d.ObjectPropId)
                .HasConstraintName("FK__Selection__Objec__395884C4");

            entity.HasOne(d => d.Owner).WithMany(p => p.SelectionList)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Selection__Owner__3F115E1A");
        });

        modelBuilder.Entity<TempData>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TempData__3214EC07ADC91FBE");

            entity.HasIndex(e => e.IX1, "IX_DT_01");

            entity.HasIndex(e => e.IX2, "IX_DT_02");

            entity.HasIndex(e => e.IX3, "IX_DT_03");

            entity.HasIndex(e => e.ExternalId, "IX_DT_EX");

            entity.HasIndex(e => e.SecondaryId, "IX_DT_SE");

            entity.HasIndex(e => e.Code, "IX_TD_CD");

            entity.HasIndex(e => e.Name, "IX_TD_NM");

            entity.HasIndex(e => e.ProcessName, "IX_TD_PN");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code)
                .HasMaxLength(240)
                .IsUnicode(false);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.D01).IsUnicode(false);
            entity.Property(e => e.D02).IsUnicode(false);
            entity.Property(e => e.D03).IsUnicode(false);
            entity.Property(e => e.D04).IsUnicode(false);
            entity.Property(e => e.D05).IsUnicode(false);
            entity.Property(e => e.D06).IsUnicode(false);
            entity.Property(e => e.D07).IsUnicode(false);
            entity.Property(e => e.D08).IsUnicode(false);
            entity.Property(e => e.D09).IsUnicode(false);
            entity.Property(e => e.D10).IsUnicode(false);
            entity.Property(e => e.D100).IsUnicode(false);
            entity.Property(e => e.D101).IsUnicode(false);
            entity.Property(e => e.D102).IsUnicode(false);
            entity.Property(e => e.D103).IsUnicode(false);
            entity.Property(e => e.D104).IsUnicode(false);
            entity.Property(e => e.D105).IsUnicode(false);
            entity.Property(e => e.D106).IsUnicode(false);
            entity.Property(e => e.D107).IsUnicode(false);
            entity.Property(e => e.D108).IsUnicode(false);
            entity.Property(e => e.D109).IsUnicode(false);
            entity.Property(e => e.D11).IsUnicode(false);
            entity.Property(e => e.D110).IsUnicode(false);
            entity.Property(e => e.D111).IsUnicode(false);
            entity.Property(e => e.D112).IsUnicode(false);
            entity.Property(e => e.D113).IsUnicode(false);
            entity.Property(e => e.D114).IsUnicode(false);
            entity.Property(e => e.D115).IsUnicode(false);
            entity.Property(e => e.D116).IsUnicode(false);
            entity.Property(e => e.D117).IsUnicode(false);
            entity.Property(e => e.D118).IsUnicode(false);
            entity.Property(e => e.D119).IsUnicode(false);
            entity.Property(e => e.D12).IsUnicode(false);
            entity.Property(e => e.D120).IsUnicode(false);
            entity.Property(e => e.D121).IsUnicode(false);
            entity.Property(e => e.D122).IsUnicode(false);
            entity.Property(e => e.D123).IsUnicode(false);
            entity.Property(e => e.D124).IsUnicode(false);
            entity.Property(e => e.D125).IsUnicode(false);
            entity.Property(e => e.D126).IsUnicode(false);
            entity.Property(e => e.D127).IsUnicode(false);
            entity.Property(e => e.D128).IsUnicode(false);
            entity.Property(e => e.D129).IsUnicode(false);
            entity.Property(e => e.D13).IsUnicode(false);
            entity.Property(e => e.D130).IsUnicode(false);
            entity.Property(e => e.D131).IsUnicode(false);
            entity.Property(e => e.D132).IsUnicode(false);
            entity.Property(e => e.D133).IsUnicode(false);
            entity.Property(e => e.D134).IsUnicode(false);
            entity.Property(e => e.D135).IsUnicode(false);
            entity.Property(e => e.D136).IsUnicode(false);
            entity.Property(e => e.D137).IsUnicode(false);
            entity.Property(e => e.D138).IsUnicode(false);
            entity.Property(e => e.D139).IsUnicode(false);
            entity.Property(e => e.D14).IsUnicode(false);
            entity.Property(e => e.D140).IsUnicode(false);
            entity.Property(e => e.D141).IsUnicode(false);
            entity.Property(e => e.D142).IsUnicode(false);
            entity.Property(e => e.D143).IsUnicode(false);
            entity.Property(e => e.D144).IsUnicode(false);
            entity.Property(e => e.D145).IsUnicode(false);
            entity.Property(e => e.D146).IsUnicode(false);
            entity.Property(e => e.D147).IsUnicode(false);
            entity.Property(e => e.D148).IsUnicode(false);
            entity.Property(e => e.D149).IsUnicode(false);
            entity.Property(e => e.D15).IsUnicode(false);
            entity.Property(e => e.D150).IsUnicode(false);
            entity.Property(e => e.D151).IsUnicode(false);
            entity.Property(e => e.D152).IsUnicode(false);
            entity.Property(e => e.D153).IsUnicode(false);
            entity.Property(e => e.D154).IsUnicode(false);
            entity.Property(e => e.D155).IsUnicode(false);
            entity.Property(e => e.D156).IsUnicode(false);
            entity.Property(e => e.D157).IsUnicode(false);
            entity.Property(e => e.D158).IsUnicode(false);
            entity.Property(e => e.D159).IsUnicode(false);
            entity.Property(e => e.D16).IsUnicode(false);
            entity.Property(e => e.D160).IsUnicode(false);
            entity.Property(e => e.D161).IsUnicode(false);
            entity.Property(e => e.D162).IsUnicode(false);
            entity.Property(e => e.D163).IsUnicode(false);
            entity.Property(e => e.D164).IsUnicode(false);
            entity.Property(e => e.D165).IsUnicode(false);
            entity.Property(e => e.D166).IsUnicode(false);
            entity.Property(e => e.D167).IsUnicode(false);
            entity.Property(e => e.D168).IsUnicode(false);
            entity.Property(e => e.D169).IsUnicode(false);
            entity.Property(e => e.D17).IsUnicode(false);
            entity.Property(e => e.D170).IsUnicode(false);
            entity.Property(e => e.D171).IsUnicode(false);
            entity.Property(e => e.D172).IsUnicode(false);
            entity.Property(e => e.D173).IsUnicode(false);
            entity.Property(e => e.D174).IsUnicode(false);
            entity.Property(e => e.D175).IsUnicode(false);
            entity.Property(e => e.D176).IsUnicode(false);
            entity.Property(e => e.D177).IsUnicode(false);
            entity.Property(e => e.D178).IsUnicode(false);
            entity.Property(e => e.D179).IsUnicode(false);
            entity.Property(e => e.D18).IsUnicode(false);
            entity.Property(e => e.D180).IsUnicode(false);
            entity.Property(e => e.D181).IsUnicode(false);
            entity.Property(e => e.D182).IsUnicode(false);
            entity.Property(e => e.D183).IsUnicode(false);
            entity.Property(e => e.D184).IsUnicode(false);
            entity.Property(e => e.D185).IsUnicode(false);
            entity.Property(e => e.D186).IsUnicode(false);
            entity.Property(e => e.D187).IsUnicode(false);
            entity.Property(e => e.D188).IsUnicode(false);
            entity.Property(e => e.D189).IsUnicode(false);
            entity.Property(e => e.D19).IsUnicode(false);
            entity.Property(e => e.D190).IsUnicode(false);
            entity.Property(e => e.D191).IsUnicode(false);
            entity.Property(e => e.D192).IsUnicode(false);
            entity.Property(e => e.D193).IsUnicode(false);
            entity.Property(e => e.D194).IsUnicode(false);
            entity.Property(e => e.D195).IsUnicode(false);
            entity.Property(e => e.D196).IsUnicode(false);
            entity.Property(e => e.D197).IsUnicode(false);
            entity.Property(e => e.D198).IsUnicode(false);
            entity.Property(e => e.D199).IsUnicode(false);
            entity.Property(e => e.D20).IsUnicode(false);
            entity.Property(e => e.D200).IsUnicode(false);
            entity.Property(e => e.D201).IsUnicode(false);
            entity.Property(e => e.D202).IsUnicode(false);
            entity.Property(e => e.D203).IsUnicode(false);
            entity.Property(e => e.D204).IsUnicode(false);
            entity.Property(e => e.D205).IsUnicode(false);
            entity.Property(e => e.D206).IsUnicode(false);
            entity.Property(e => e.D207).IsUnicode(false);
            entity.Property(e => e.D208).IsUnicode(false);
            entity.Property(e => e.D209).IsUnicode(false);
            entity.Property(e => e.D21).IsUnicode(false);
            entity.Property(e => e.D210).IsUnicode(false);
            entity.Property(e => e.D211).IsUnicode(false);
            entity.Property(e => e.D212).IsUnicode(false);
            entity.Property(e => e.D213).IsUnicode(false);
            entity.Property(e => e.D214).IsUnicode(false);
            entity.Property(e => e.D215).IsUnicode(false);
            entity.Property(e => e.D216).IsUnicode(false);
            entity.Property(e => e.D217).IsUnicode(false);
            entity.Property(e => e.D218).IsUnicode(false);
            entity.Property(e => e.D219).IsUnicode(false);
            entity.Property(e => e.D22).IsUnicode(false);
            entity.Property(e => e.D220).IsUnicode(false);
            entity.Property(e => e.D23).IsUnicode(false);
            entity.Property(e => e.D24).IsUnicode(false);
            entity.Property(e => e.D25).IsUnicode(false);
            entity.Property(e => e.D26).IsUnicode(false);
            entity.Property(e => e.D27).IsUnicode(false);
            entity.Property(e => e.D28).IsUnicode(false);
            entity.Property(e => e.D29).IsUnicode(false);
            entity.Property(e => e.D30).IsUnicode(false);
            entity.Property(e => e.D31).IsUnicode(false);
            entity.Property(e => e.D32).IsUnicode(false);
            entity.Property(e => e.D33).IsUnicode(false);
            entity.Property(e => e.D34).IsUnicode(false);
            entity.Property(e => e.D35).IsUnicode(false);
            entity.Property(e => e.D36).IsUnicode(false);
            entity.Property(e => e.D37).IsUnicode(false);
            entity.Property(e => e.D38).IsUnicode(false);
            entity.Property(e => e.D39).IsUnicode(false);
            entity.Property(e => e.D40).IsUnicode(false);
            entity.Property(e => e.D41).IsUnicode(false);
            entity.Property(e => e.D42).IsUnicode(false);
            entity.Property(e => e.D43).IsUnicode(false);
            entity.Property(e => e.D44).IsUnicode(false);
            entity.Property(e => e.D45).IsUnicode(false);
            entity.Property(e => e.D46).IsUnicode(false);
            entity.Property(e => e.D47).IsUnicode(false);
            entity.Property(e => e.D48).IsUnicode(false);
            entity.Property(e => e.D49).IsUnicode(false);
            entity.Property(e => e.D50).IsUnicode(false);
            entity.Property(e => e.D51).IsUnicode(false);
            entity.Property(e => e.D52).IsUnicode(false);
            entity.Property(e => e.D53).IsUnicode(false);
            entity.Property(e => e.D54).IsUnicode(false);
            entity.Property(e => e.D55).IsUnicode(false);
            entity.Property(e => e.D56).IsUnicode(false);
            entity.Property(e => e.D57).IsUnicode(false);
            entity.Property(e => e.D58).IsUnicode(false);
            entity.Property(e => e.D59).IsUnicode(false);
            entity.Property(e => e.D60).IsUnicode(false);
            entity.Property(e => e.D61).IsUnicode(false);
            entity.Property(e => e.D62).IsUnicode(false);
            entity.Property(e => e.D63).IsUnicode(false);
            entity.Property(e => e.D64).IsUnicode(false);
            entity.Property(e => e.D65).IsUnicode(false);
            entity.Property(e => e.D66).IsUnicode(false);
            entity.Property(e => e.D67).IsUnicode(false);
            entity.Property(e => e.D68).IsUnicode(false);
            entity.Property(e => e.D69).IsUnicode(false);
            entity.Property(e => e.D70).IsUnicode(false);
            entity.Property(e => e.D71).IsUnicode(false);
            entity.Property(e => e.D72).IsUnicode(false);
            entity.Property(e => e.D73).IsUnicode(false);
            entity.Property(e => e.D74).IsUnicode(false);
            entity.Property(e => e.D75).IsUnicode(false);
            entity.Property(e => e.D76).IsUnicode(false);
            entity.Property(e => e.D77).IsUnicode(false);
            entity.Property(e => e.D78).IsUnicode(false);
            entity.Property(e => e.D79).IsUnicode(false);
            entity.Property(e => e.D80).IsUnicode(false);
            entity.Property(e => e.D81).IsUnicode(false);
            entity.Property(e => e.D82).IsUnicode(false);
            entity.Property(e => e.D83).IsUnicode(false);
            entity.Property(e => e.D84).IsUnicode(false);
            entity.Property(e => e.D85).IsUnicode(false);
            entity.Property(e => e.D86).IsUnicode(false);
            entity.Property(e => e.D87).IsUnicode(false);
            entity.Property(e => e.D88).IsUnicode(false);
            entity.Property(e => e.D89).IsUnicode(false);
            entity.Property(e => e.D90).IsUnicode(false);
            entity.Property(e => e.D91).IsUnicode(false);
            entity.Property(e => e.D92).IsUnicode(false);
            entity.Property(e => e.D93).IsUnicode(false);
            entity.Property(e => e.D94).IsUnicode(false);
            entity.Property(e => e.D95).IsUnicode(false);
            entity.Property(e => e.D96).IsUnicode(false);
            entity.Property(e => e.D97).IsUnicode(false);
            entity.Property(e => e.D98).IsUnicode(false);
            entity.Property(e => e.D99).IsUnicode(false);
            entity.Property(e => e.ExternalId)
                .HasMaxLength(240)
                .IsUnicode(false);
            entity.Property(e => e.F01).HasColumnType("datetime");
            entity.Property(e => e.F02).HasColumnType("datetime");
            entity.Property(e => e.F03).HasColumnType("datetime");
            entity.Property(e => e.F04).HasColumnType("datetime");
            entity.Property(e => e.F05).HasColumnType("datetime");
            entity.Property(e => e.IX1)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.IX2)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.IX3)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.M01).IsUnicode(false);
            entity.Property(e => e.M02).IsUnicode(false);
            entity.Property(e => e.M03).IsUnicode(false);
            entity.Property(e => e.M04).IsUnicode(false);
            entity.Property(e => e.M05).IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(240)
                .IsUnicode(false);
            entity.Property(e => e.ProcessDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProcessName)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.SecondaryId)
                .HasMaxLength(240)
                .IsUnicode(false);
            entity.Property(e => e.T01)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.T02)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.T03)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.T04)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.T05)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.T06)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.T07)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.T08)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.T09)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.T10)
                .HasMaxLength(120)
                .IsUnicode(false);
        });

        modelBuilder.Entity<UserObject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserObje__3214EC07464610B7");

            entity.HasIndex(e => new { e.UserId, e.ObjectId }, "UQ__UserObje__1E2ED5649A266DF2").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Access).HasDefaultValue(1);

            entity.HasOne(d => d.Object).WithMany(p => p.UserObject)
                .HasForeignKey(d => d.ObjectId)
                .HasConstraintName("FK__UserObjec__Objec__73852659");

            entity.HasOne(d => d.User).WithMany(p => p.UserObject)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserObjec__UserI__72910220");
        });

        modelBuilder.Entity<UserRegistry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRegi__3214EC07400D5740");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Registry).WithMany(p => p.UserRegistry)
                .HasForeignKey(d => d.RegistryId)
                .HasConstraintName("FK__UserRegis__Regis__336AA144");

            entity.HasOne(d => d.User).WithMany(p => p.UserRegistry)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRegis__UserI__32767D0B");
        });

        modelBuilder.Entity<UserReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRepo__3214EC079CD0D741");

            entity.HasIndex(e => new { e.UserId, e.ReportId }, "UQ__UserRepo__5AD318CD83989A5F").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Access).HasDefaultValue(1);

            entity.HasOne(d => d.Report).WithMany(p => p.UserReport)
                .HasForeignKey(d => d.ReportId)
                .HasConstraintName("FK__UserRepor__Repor__1D7B6025");

            entity.HasOne(d => d.User).WithMany(p => p.UserReport)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRepor__UserI__1C873BEC");
        });

        modelBuilder.Entity<UserView>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserView__3214EC07B19B177E");

            entity.HasIndex(e => new { e.UserId, e.ViewId }, "UQ__UserView__666BBD829E820CD4").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Access).HasDefaultValue(1);

            entity.HasOne(d => d.User).WithMany(p => p.UserView)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserView__UserId__7A3223E8");

            entity.HasOne(d => d.View).WithMany(p => p.UserView)
                .HasForeignKey(d => d.ViewId)
                .HasConstraintName("FK__UserView__ViewId__7B264821");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC0764B7EDAC");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("U_AI");
                    tb.HasTrigger("U_AU");
                });

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105347F1063F1").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Avatar).IsUnicode(false);
            entity.Property(e => e.CreationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(320)
                .IsUnicode(false);
            entity.Property(e => e.ExternalId)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Folio)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasComputedColumnSql("('U-'+right('0000000000'+CONVERT([varchar](10),[Number]),(10)))", true);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LandlinePhone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MobilePhone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ModificationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .IsUnicode(false);
            entity.Property(e => e.Number).ValueGeneratedOnAdd();
            entity.Property(e => e.Password)
                .HasMaxLength(120)
                .IsUnicode(false)
                .HasDefaultValue("0000");
            entity.Property(e => e.Token).IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(320)
                .IsUnicode(false)
                .HasDefaultValueSql("(((CONVERT([varchar](3),datepart(dayofyear,getdate()))+'_')+CONVERT([varchar](3),datepart(millisecond,getdate())))+'@pladic.app')");

            entity.HasOne(d => d.Owner).WithMany(p => p.InverseOwner)
                .HasForeignKey(d => d.OwnerId)
                .HasConstraintName("FK__Users__OwnerId__44FF419A");

            entity.HasOne(d => d.Profile).WithMany(p => p.Users)
                .HasForeignKey(d => d.ProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__ProfileId__440B1D61");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
