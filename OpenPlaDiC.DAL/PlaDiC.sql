/* DATABASE NORMALIZATION TO ENGLISH
   - Table 'Usuario' -> 'Users'
   - Table 'Vista'   -> 'CustomView'
   - All other objects translated and structure preserved.
*/

create table Registry(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120),
Folio varchar(60) not null,
Object varchar(60),
Extra varchar(200),
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate()) 

go

create table Event(
Id uniqueidentifier not null primary key default newid(),
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'EV-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Type varchar(120) not null,
ProcedureName varchar(120),
Tag varchar(120),
SessionInfo varchar(max),
EventInfo varchar(max),
DateTime datetime not null default getdate(),
Reference varchar(120))

go

create table Parameter(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120) not null,
Label varchar(120) not null,
Value varchar(max),
GroupName varchar(120),
Extra varchar(200),
Type int not null default 0,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate()) 

go

create table Profile(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'P-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Description varchar(max) null,
IsAvailable bit not null default 1,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier null,
unique(Name)
) 

go

CREATE TRIGGER P_AI
ON Profile
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
	insert into Registry(Id, Name, Folio, Object) 
	select i.Id, i.Name, i.Folio, 'Profile'
	from inserted i
END

go

CREATE TRIGGER P_AU 
ON Profile
AFTER UPDATE 
AS 
BEGIN 
	SET NOCOUNT ON; 
	update Registry 
		set Registry.Name = i.Name
	from Registry r 
	inner join inserted i on i.Id = r.Id 
END

go

insert into Profile(Id, Name, Description) values('10000000-0000-0000-0000-000000000000','Administrator','Account Administrator')
go
insert into Profile(Id, Name, Description) values('10000000-0000-0000-0000-000000000001','General','General User')
go

create table Users(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'U-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
ExternalId varchar(200),
IsMaster bit not null default 0,
IsConfirmed bit not null default 0,
IsActive bit not null default 1,
Email varchar(320) not null,
Username varchar(320) not null default convert(varchar(3),DATEPART(dayofyear, getdate()))+'_'+convert(varchar(3),DATEPART(MILLISECOND, getdate()))+'@pladic.app',
Password varchar(120) not null default '0000',
MobilePhone varchar(20),
LandlinePhone varchar(20),
Avatar varchar(max),
Token varchar(max),
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
ProfileId uniqueidentifier not null references Profile,
OwnerId uniqueidentifier references Users,
unique(Email)
) 
go

CREATE TRIGGER U_AI
ON Users
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
	insert into Registry(Id, Name, Folio, Object, Extra) 
	select i.Id, i.Name, i.Folio, 'Users', i.Email+'|'+isnull(i.MobilePhone,'')+'|'+isnull(i.LandlinePhone,'')
	from inserted i
END

go

CREATE TRIGGER U_AU 
ON Users
AFTER UPDATE 
AS 
BEGIN 
	SET NOCOUNT ON; 
	update Registry 
		set Registry.Name = i.Name, Registry.Extra = i.Email+'|'+isnull(i.MobilePhone,'')+'|'+isnull(i.LandlinePhone,'')
	from Registry r 
	inner join inserted i on i.Id = r.Id 
END

go

create table Message(
Id uniqueidentifier not null primary key default newid(),
Subject varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Content varchar(max) not null,
IsNew bit not null default 1,
IsNotification bit not null default 1,
IsTextOnly bit not null default 1,
SendEmail bit not null default 0,
SendSMS bit not null default 0,
RegistryId uniqueidentifier references Registry,
CreationDate datetime not null default getdate(),
ReadDate datetime,
SenderId uniqueidentifier not null references Users,
OwnerId uniqueidentifier not null references Users)

go

create table Project(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'PY-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Status int not null default 0,  -- New, In execution, Paused, Finished, Cancelled
Description varchar(max),
StartDate date,
EndDate date,
Progress numeric(7,4) not null default 0,
Cost numeric(15,4) not null default 0,
Color varchar(120) default '#ffffff',
Image varchar(max),
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users
unique(Name)
) 

go

CREATE TRIGGER PY_AI
ON Project
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
	insert into Registry(Id, Name, Folio, Object) 
	select i.Id, i.Name, i.Folio, 'Project'
	from inserted i
END

go

CREATE TRIGGER PY_AU 
ON Project
AFTER UPDATE 
AS 
BEGIN 
	SET NOCOUNT ON; 
	update Registry 
		set Registry.Name = i.Name
	from Registry r 
	inner join inserted i on i.Id = r.Id 
END

go

create table CustomTask(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'T-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Type int not null default 0,  -- CustomTask, Milestone
Status int not null default 0,  -- New, In execution, Paused, Finished, Cancelled
Tag varchar(120),
Block int not null default 0,
Priority int not null default 0,
ProjectId uniqueidentifier references Project,
ParentCustomTaskId uniqueidentifier references CustomTask,
Description varchar(max),
StartDate date,
EndDate date,
Progress numeric(7,4) not null default 0,
Stage int not null default 0, -- Undefined, Planning, Analysis, Implementation, Testing, Release
Cost numeric(15,4) not null default 0,
Hours numeric(15,4) not null default 0,
Color varchar(120) default '#ffffff',
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users
unique(ProjectId, ParentCustomTaskId, Name)
) 

go

CREATE TRIGGER T_AI
ON CustomTask
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
	insert into Registry(Id, Name, Folio, Object) 
	select i.Id, i.Name, i.Folio, 'CustomTask'
	from inserted i
END

go

CREATE TRIGGER T_AU 
ON CustomTask
AFTER UPDATE 
AS 
BEGIN 
	SET NOCOUNT ON; 
	update Registry 
		set Registry.Name = i.Name
	from Registry r 
	inner join inserted i on i.Id = r.Id 
END

go

create table CustomTaskUser(
Id uniqueidentifier not null primary key default newid(),
IsExternal bit not null default 0,
IsActive bit not null default 1,
CanModifyCustomTask bit not null default 0,
CanPublishLog bit not null default 1,
Cost numeric(15,4) not null default 0,
Weight int not null default 0,
StartDate date,
EndDate date,
CustomTaskId uniqueidentifier not null references CustomTask on delete cascade,
UserId uniqueidentifier references Users on delete cascade,
Email varchar(240),
MobilePhone varchar(20),
unique(CustomTaskId, UserId)
)

go 

create table CustomTaskDep(
Id uniqueidentifier not null primary key default newid(),
CustomTaskId uniqueidentifier not null references CustomTask on delete cascade,
CustomTaskDepId uniqueidentifier not null references CustomTask,
unique(CustomTaskId, CustomTaskDepId)
)

go

create table MetadataObject(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'O-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Prefix varchar(10) not null,
Label varchar(120) not null,
Icon varchar(120),
UseName bit not null default 1,
NameLabel varchar(120) not null default 'Name',
NameHelp varchar(120) not null default 'Name assigned to the new record',
IsLocked bit not null default 0,
IsVisible bit not null default 1,
IsDetail bit not null default 0,
IsRelated bit not null default 1,
ControlledAccess bit not null default 1,
CustomStatement bit not null default 0,
ListStatement varchar(max),
RelatedStatement varchar(max),
FieldsStatement varchar(max), 
FilterStatement varchar(max),
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users,
ListRecordsCount int not null default -1,
RelatedRecordsCount int not null default -1,
TriggerAI varchar(max) NULL,
TriggerBI varchar(max) NULL,
TriggerAU varchar(max) NULL,
TriggerBU varchar(max) NULL,
TriggerAD varchar(max) NULL,
TriggerBD varchar(max) NULL,
unique(Name),
unique(Prefix)
) 

go

CREATE TRIGGER O_AI
ON MetadataObject
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
	insert into Registry(Id, Name, Folio, Object, Extra) 
	select i.Id, i.Name, i.Folio, 'MetadataObject', i.Label
	from inserted i
END

go

create table ObjectProp(
Id uniqueidentifier not null primary key default newid(),
ObjectId uniqueidentifier not null references MetadataObject on delete cascade,
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'OP-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Label varchar(120) not null,
Help varchar(480),
Type int not null default 0,
DefaultValue varchar(120),
RegExp varchar(480),
MaxLng int not null default 0,
MaxVal numeric(15,4) not null default 0,
MinVal numeric(15,4) not null default 0,
IsRequired bit not null default 0,
IsUnique bit not null default 0,
RefObjectId uniqueidentifier references MetadataObject,
RelatedStatement varchar(max),
FieldsStatement varchar(max), 
Formula varchar(max),
GroupName varchar(120) not null,
Sequence int not null default 0,
ColumnPos int not null default 0,
RowPos int not null default 0,
OptionVal int not null default 0,
IsVisible bit not null default 1,
IsEditable bit not null default 1,
InList bit not null default 0,
InRelated bit not null default 0,
ApplyFilter bit default 0 not null,
GenerateAccess bit not null default 0,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users,
unique(ObjectId, Name)
) 

go

create table ObjectList(
Id uniqueidentifier not null primary key default newid(),
ObjectId uniqueidentifier not null references MetadataObject on delete cascade,
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'OL-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Value varchar(max) not null,
GroupName varchar(120),
Extra varchar(200),
Type int not null default 0,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users)

go

create table SelectionList(
Id uniqueidentifier not null primary key default newid(),
ObjectPropId uniqueidentifier not null references ObjectProp on delete cascade,
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'LS-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Value varchar(max) not null,
GroupName varchar(120),
Extra varchar(200),
Type int not null default 0,
Color varchar(20),
Sequence int not null default 0,
IsActive bit not null default 1,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users)

go

create table GenericList(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'LG-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Type int not null default 0,
IsActive bit not null default 1,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users)

go

create table GenericListVal(
Id uniqueidentifier not null primary key default newid(),
GenericListId uniqueidentifier not null references GenericList on delete cascade,
Name varchar(120) not null,
Number int IDENTITY(1,1) NOT NULL,
Folio AS 'LV-'+RIGHT('0000000000' + CONVERT(varchar(10),Number), 10) PERSISTED,
Value varchar(max) not null,
GroupName varchar(120),
Extra varchar(200),
Type int not null default 0,
Color varchar(20),
Sequence int not null default 0,
IsActive bit not null default 1,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users)

go

CREATE TRIGGER OP_AD
ON ObjectProp
FOR DELETE
AS
BEGIN
    SET NOCOUNT ON;
	declare @Prop varchar(120), @TableName varchar(120), @Type int, @Def varchar(120), @Un bit
	
	select 
		@TableName = o.Name, 
		@Prop = i.Name,
		@Type = i.Type,
		@Def  = i.DefaultValue,
		@Un = i.IsUnique
	from deleted i
	inner join MetadataObject o on o.Id = i.ObjectId

	declare @sqlCmd varchar(max)
	if(@Type = 10 or @Type = 20)
	begin
		set @sqlCmd = 'alter table '+@TableName+' drop CONSTRAINT FK__'+@TableName+'__'+@Prop
		exec(@sqlCmd)
	end

	if(@Def is not null)
	begin
		set @sqlCmd = 'alter table '+@TableName+' drop CONSTRAINT DF__'+@TableName+'__'+@Prop
		exec(@sqlCmd)
	end

	if(@Un = 1)
	begin
		set @sqlCmd = 'alter table '+@TableName+' drop CONSTRAINT AK__'+@TableName+'__'+@Prop
		exec(@sqlCmd)
	end

	set @sqlCmd = 'alter table '+@TableName+' drop column '+@Prop
	exec(@sqlCmd)
END

go

CREATE PROCEDURE CreateObject
@Name varchar(120),
@Label varchar(120),
@Prefix varchar(10),
@Icon varchar(120),
@OwnerId uniqueidentifier,
@UseName bit = 1,
@NameLabel varchar(120) = 'Name'
AS
BEGIN
	SET NOCOUNT ON;
    insert into MetadataObject(Name, Prefix, IsLocked, Label, IsVisible, OwnerId, UseName, NameLabel, ListStatement, Icon) 
	values(@Name, @Prefix, 0, @Label, 1, @OwnerId, @UseName, @NameLabel, 'select Id, Folio, Name ['+ @NameLabel+'] from '+@Name, @Icon)

	declare @sqlCmd varchar(max)
	set @sqlCmd = 
		' create table '+@Name+'( '+
		' Id uniqueidentifier not null primary key default newid(), '+
		' Name varchar(120)'+ case @UseName when 1 then ' not null,' else ',' end+
		' Number int IDENTITY(1,1) NOT NULL, '+
		' Folio AS '''+upper(@Prefix)+'-''+RIGHT(''0000000000'' + CONVERT(varchar(10),Number), 10) PERSISTED, '+
		' IsAvailable bit not null default 1, '+
		' IsDeleted bit not null default 0, '+
		' Reference varchar(120), '+
		' CreationDate datetime not null default getdate(), '+
		' ModificationDate datetime not null default getdate(), '+
		' OwnerId uniqueidentifier not null references Users default ''00000000-0000-0000-0000-000000000000'' '+
		' ) '

	exec(@sqlCmd)

	set @sqlCmd = 
	' CREATE TRIGGER '+upper(@Prefix)+'_AI ON '+@Name+' AFTER INSERT AS BEGIN SET NOCOUNT ON; '+
	' insert into Registry(Id, Name, Folio, Object, Extra) '+
	' select i.Id, '+case @UseName when 1 then ' i.Name' else ' i.Folio' end+', i.Folio, '''+@Name+''', i.Reference from inserted i END '
	exec(@sqlCmd)

	set @sqlCmd = 
	' CREATE TRIGGER '+upper(@Prefix)+'_AU ON '+@Name+' AFTER UPDATE AS BEGIN SET NOCOUNT ON; '+
	' update Registry set Registry.Name = i.Name, Registry.Extra = i.Reference from Registry r inner join inserted i on i.Id = r.Id END '
	exec(@sqlCmd)
END

go

CREATE FUNCTION GetIdByFolio
(
	@Folio varchar(20)
)
RETURNS uniqueidentifier
AS
BEGIN
	declare @res uniqueidentifier
	if(substring(@Folio,1,3) = 'LS-')
		select @res = r.Id from SelectionList r where r.Folio = @Folio
	else
		select @res = r.Id from Registry r where r.Folio = @Folio
	RETURN @res
END
GO

create table CustomView(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120) not null,
Label varchar(120) not null,
Icon varchar(120),
ObjectId uniqueidentifier references MetadataObject,
Content varchar(max) not null,
IsAvailable bit not null default 1,
RequiresSession bit not null default 1,
AvailableWithSession bit not null default 1,
AvailableWithoutSession bit not null default 1,
InMenu bit not null default 0,
Type varchar(10) not null default 'VIEW',
UseAsStart bit not null default 0,
ControlledAccess bit not null default 1,
IsActive bit not null default 0,
StartExecutionDate datetime,
EndExecutionDate datetime,
StartExecutionTime time,
EndExecutionTime time,
LastExecutionDate datetime,
NextExecutionDate datetime,
FrequencyMinutes int not null default 0,
Repetitions int not null default 0,
Mon bit not null default 1,
Tue bit not null default 1,
Wed bit not null default 1,
Thu bit not null default 1,
Fri bit not null default 1,
Sat bit not null default 1,
Sun bit not null default 1,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users,
unique(Name)
)

go

CREATE PROCEDURE GetObjectList
@ObjectId uniqueidentifier,
@Fields varchar(max) = null,
@Filter varchar(max) = null,
@FilterRef varchar(max) = null,
@Join varchar(max) = null,
@Order varchar(max) = null,
@MaxRecords int = null,
@OwnerId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;
	declare @sqlCmd varchar(max), @Name varchar(60), @Custom bit
	select @Name = Name, @sqlCmd = ListStatement, @Custom = CustomStatement from MetadataObject where Id = @ObjectId
    
	if (@Custom = 1)
	begin
		set @sqlCmd = replace(@sqlCmd, '_USR_', @OwnerId)
		set @sqlCmd = replace(@sqlCmd, 'select ', 'select top '+isnull(cast(@MaxRecords as varchar), '100')+' ')
	end
	else
	begin
		set @sqlCmd = replace(@sqlCmd, 'select ', 'select top '+isnull(cast(@MaxRecords as varchar), '100')+' ')
		if(@FilterRef is not null)
		  select @sqlCmd = case when CHARINDEX('order by',@sqlCmd) > 0 then replace(@sqlCmd,'order by',@FilterRef+' order by' ) else @sqlCmd+' '+@FilterRef end  

		if(@Fields is not null or @Filter is not null or @Join is not null or @Order is not null)
			set @sqlCmd = ' select '+isnull(@Fields,'*')+' from '+@Name+' '+isnull(@Join,'')+' '+isnull(@Filter,'')+' '+ isnull(@Order,' order by '+@Name+'.Folio ')
	end
	exec(@sqlCmd)
END

go

CREATE PROCEDURE GetListGeneric
@Object varchar(max),
@Fields varchar(max) = null,
@Filter varchar(max) = null,
@Join varchar(max) = null,
@Order varchar(max) = null,
@Group varchar(max) = null,
@OwnerId uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;
	declare @sqlCmd varchar(max)
	set @sqlCmd = ' select '+isnull(@Fields,'*')+' from '+@Object+' '+isnull(@Join,'')+' '+isnull(@Filter,'')+' '+ isnull(@Group,'')+' '+ isnull(@Order,'')
	exec(@sqlCmd)
END

go

create table UserObject(
Id uniqueidentifier not null primary key default newid(),
UserId uniqueidentifier not null references Users on delete cascade,
ObjectId uniqueidentifier not null references MetadataObject on delete cascade,
Access int not null default 1,
InMain bit not null default 0,
unique(UserId, ObjectId)
) 

go

create table UserView(
Id uniqueidentifier not null primary key default newid(),
UserId uniqueidentifier not null references Users on delete cascade,
ViewId uniqueidentifier not null references CustomView on delete cascade,
Access int not null default 1,
unique(UserId, ViewId)
) 

go

create table ProfileObject(
Id uniqueidentifier not null primary key default newid(),
ProfileId uniqueidentifier not null references Profile on delete cascade,
ObjectId uniqueidentifier not null references MetadataObject on delete cascade,
Access int not null default 1,
InMain bit not null default 0,
unique(ProfileId, ObjectId)
) 

go

create table ProfileView(
Id uniqueidentifier not null primary key default newid(),
ProfileId uniqueidentifier not null references Profile on delete cascade,
ViewId uniqueidentifier not null references CustomView on delete cascade,
Access int not null default 1,
unique(ProfileId, ViewId)
) 

go

create table Report(
Id uniqueidentifier not null primary key default newid(),
Name varchar(120) not null,
Label varchar(120) not null,
Icon varchar(120),
Query varchar(max) not null,
IsAvailable bit not null default 1,
RequiresSession bit not null default 1,
AvailableWithSession bit not null default 1,
AvailableWithoutSession bit not null default 1,
InMenu bit not null default 0,
Type varchar(10) not null default 'LISTING',
ControlledAccess bit not null default 1,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users,
unique(Name)
)

go

create table UserReport(
Id uniqueidentifier not null primary key default newid(),
UserId uniqueidentifier not null references Users on delete cascade,
ReportId uniqueidentifier not null references Report on delete cascade,
Access int not null default 1,
unique(UserId, ReportId)
) 

go

create table ReportParam(
Id uniqueidentifier not null primary key default newid(),
ReportId uniqueidentifier not null references Report on delete cascade,
Name varchar(120) not null,
Label varchar(120) not null,
Help varchar(480),
Type int not null default 0,
DefaultValue varchar(120),
RegExp varchar(480),
MaxLng int not null default 0,
MaxVal numeric(15,4) not null default 0,
MinVal numeric(15,4) not null default 0,
IsRequired bit not null default 0,
RefObjectId uniqueidentifier references MetadataObject,
Formula varchar(max),
GroupName varchar(120) not null,
Sequence int not null default 0,
ColumnPos int not null default 0,
RowPos int not null default 0,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate(),
OwnerId uniqueidentifier not null references Users,
unique(ReportId, Name)
) 

go

create table UserRegistry(
Id uniqueidentifier not null primary key default newid(),
UserId uniqueidentifier not null references Users on delete cascade,
RegistryId uniqueidentifier not null references Registry on delete cascade,
Access int not null default 0,
DenyAccess bit not null default 0,
CreationDate datetime not null default getdate(),
ModificationDate datetime not null default getdate()) 

go

-- Initial Data
insert into Users(Id, Name, Email, Username, Password, IsMaster, OwnerId, ProfileId) 
values('00000000-0000-0000-0000-000000000000','Super User', (SELECT REPLACE(lower( DB_NAME() ),'db', '' ) +'_admin@pladic.app' ) ,'admin@'+(SELECT REPLACE(lower( DB_NAME() ),'db', '' ) ) ,'0000',1,'00000000-0000-0000-0000-000000000000','10000000-0000-0000-0000-000000000000')
go

insert into MetadataObject (Id, Name, Prefix, Label, OwnerId, ListStatement) values('00000000-0000-0000-0000-000000000002', 'Profile','P','Profiles','00000000-0000-0000-0000-000000000000','Select Id, Folio, Name from Profile')
go
insert into ObjectProp (ObjectId, Name, Label, Help, Type, GroupName, OwnerId, Sequence) values('00000000-0000-0000-0000-000000000002', 'Description','Description','',7,'General','00000000-0000-0000-0000-000000000000',0)
go
insert into MetadataObject (Id, Name, Prefix, Label, OwnerId, ListStatement) values('00000000-0000-0000-0000-000000000001', 'Users','U','Users','00000000-0000-0000-0000-000000000000','Select Id, Folio, Name, Email from Users')
go
insert into ObjectProp (ObjectId, Name, Label, Help, Type, GroupName, OwnerId, Sequence, IsRequired, RefObjectId) values('00000000-0000-0000-0000-000000000001', 'Profile','Profile','Assigned Profile',10,'General','00000000-0000-0000-0000-000000000000',0,1,'00000000-0000-0000-0000-000000000002')
go

insert into MetadataObject (Id, Name, Prefix, Label, OwnerId, ListStatement) values('00000000-0000-0000-0000-000000000005', 'Project','PY','Projects','00000000-0000-0000-0000-000000000000','Select Id, Folio, Name from Project')
go
insert into ObjectProp (ObjectId, Name, Label, Help, Type, GroupName, OwnerId, Sequence, IsRequired, IsEditable, Formula) values('00000000-0000-0000-0000-000000000005', 'Status','Status','Current status of project',21, 'General','00000000-0000-0000-0000-000000000000',0,1,0,'New,In execution,Paused,Finished,Cancelled')
go

insert into Message(Subject, Content, OwnerId, SenderId, SendEmail, SendSMS) values('Welcome to Pladic','Thank you for choosing our platform.','00000000-0000-0000-0000-000000000000','00000000-0000-0000-0000-000000000000',1,1)
go

CREATE TRIGGER OP_AI
ON ObjectProp
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
	declare @Prop varchar(120), @TableName varchar(120), @TypeStr varchar(240), @TypeInt int, @RefTableName varchar(120), @Un bit
	select 
		@TableName = o.Name, @Prop = i.Name, @RefTableName = r.Name, @TypeInt = i.Type, @Un = i.IsUnique,
		@TypeStr = (case i.Type
			when 0 then 'varchar(120)' when 1 then 'int' when 2 then 'numeric(15,4)' when 3 then 'date'
			when 103 then 'datetime' when 4 then 'varchar(360)' when 5 then 'varchar(20)' when 7 then 'varchar(max)'
			when 8 then 'varchar(max)' when 10 then 'uniqueidentifier' when 11 then 'bit' when 12 then 'varchar(120)'
			when 13 then 'numeric(15,4)' when 14 then 'varchar(20)' when 20 then 'uniqueidentifier' when 21 then 'int' else 'varchar(max)' end)+
		case when i.DefaultValue is null then '' else ' CONSTRAINT DF__'+@TableName+'__'+@Prop+ ' default '''+i.DefaultValue+''' ' end+
		case i.IsRequired when 0 then '' else ' not null' end
	from inserted i inner join MetadataObject o on o.Id = i.ObjectId left join MetadataObject r on r.Id = i.RefObjectId

	declare @sqlCmd varchar(max) = 'alter table '+@TableName+' add '+@Prop+' '+@TypeStr
	exec(@sqlCmd)

	if(@TypeInt = 10)
	begin
		set @sqlCmd = 'alter table '+@TableName+' add CONSTRAINT FK__'+@TableName+'__'+@Prop+' FOREIGN KEY ('+@Prop+') REFERENCES '+@RefTableName
		exec(@sqlCmd)
	end
	if(@TypeInt = 20)
	begin
		set @sqlCmd = 'alter table '+@TableName+' add CONSTRAINT FK__'+@TableName+'__'+@Prop+' FOREIGN KEY ('+@Prop+') REFERENCES SelectionList'
		exec(@sqlCmd)
	end
	if(@Un = 1)
	begin
		set @sqlCmd = 'alter table '+@TableName+' add CONSTRAINT AK__'+@TableName+'__'+@Prop+' UNIQUE ('+@Prop+') '
		exec(@sqlCmd)
	end
END
go

-- Helper Procedures
CREATE PROCEDURE AddSelectionList
@ObjectPropId uniqueidentifier, @Name varchar(120), @Value varchar(max), @OwnerId uniqueidentifier, @IsActive bit = 1
AS
BEGIN
	SET NOCOUNT ON;
	declare @next int
	select @next = count(*) + 1 from SelectionList where ObjectPropId = @ObjectPropId
	insert into SelectionList (ObjectPropId, Name, Value, OwnerId, IsActive, Sequence) values(@ObjectPropId, isnull(@Name,'New Value'), @Value, @OwnerId, @IsActive, @next)
	select 'OK'
END
go

create table TempData(
Id uniqueidentifier not null primary key default newid(),
ExternalId varchar(240) null,
SecondaryId varchar(240) null,
RefId uniqueidentifier null,
ProcessName varchar(120) not null,
ProcessDate datetime not null default getdate(),
ProcessStage int not null default 0,
Sequence int not null default 0,
IsActive bit not null default 0,
IsSelected bit not null default 0,
Name varchar(240) null,
Code varchar(240) null,
IX1 varchar(120) null,
IX2 varchar(120) null,
IX3 varchar(120) null,
I01 integer null,
I02 integer null,
I03 integer null,
I04 integer null,
I05 integer null,
N01 integer null,
N02 integer null,
N03 integer null,
N04 integer null,
N05 integer null,
T01 varchar(120) null,
T02 varchar(120) null,
T03 varchar(120) null,
T04 varchar(120) null,
T05 varchar(120) null,
T06 varchar(120) null,
T07 varchar(120) null,
T08 varchar(120) null,
T09 varchar(120) null,
T10 varchar(120) null,
F01 datetime null,
F02 datetime null,
F03 datetime null,
F04 datetime null,
F05 datetime null,
M01 varchar(max) null,
M02 varchar(max) null,
M03 varchar(max) null,
M04 varchar(max) null,
M05 varchar(max) null,
B01 bit null,
B02 bit null,
B03 bit null,
B04 bit null,
B05 bit null,
D01 varchar(max) null,
D02 varchar(max) null,
D03 varchar(max) null,
D04 varchar(max) null,
D05 varchar(max) null,
D06 varchar(max) null,
D07 varchar(max) null,
D08 varchar(max) null,
D09 varchar(max) null,
D10 varchar(max) null,
D11 varchar(max) null,
D12 varchar(max) null,
D13 varchar(max) null,
D14 varchar(max) null,
D15 varchar(max) null,
D16 varchar(max) null,
D17 varchar(max) null,
D18 varchar(max) null,
D19 varchar(max) null,
D20 varchar(max) null,
D21 varchar(max) null,
D22 varchar(max) null,
D23 varchar(max) null,
D24 varchar(max) null,
D25 varchar(max) null,
D26 varchar(max) null,
D27 varchar(max) null,
D28 varchar(max) null,
D29 varchar(max) null,
D30 varchar(max) null,
D31 varchar(max) null,
D32 varchar(max) null,
D33 varchar(max) null,
D34 varchar(max) null,
D35 varchar(max) null,
D36 varchar(max) null,
D37 varchar(max) null,
D38 varchar(max) null,
D39 varchar(max) null,
D40 varchar(max) null,
D41 varchar(max) null,
D42 varchar(max) null,
D43 varchar(max) null,
D44 varchar(max) null,
D45 varchar(max) null,
D46 varchar(max) null,
D47 varchar(max) null,
D48 varchar(max) null,
D49 varchar(max) null,
D50 varchar(max) null,
D51 varchar(max) null,
D52 varchar(max) null,
D53 varchar(max) null,
D54 varchar(max) null,
D55 varchar(max) null,
D56 varchar(max) null,
D57 varchar(max) null,
D58 varchar(max) null,
D59 varchar(max) null,
D60 varchar(max) null,
D61 varchar(max) null,
D62 varchar(max) null,
D63 varchar(max) null,
D64 varchar(max) null,
D65 varchar(max) null,
D66 varchar(max) null,
D67 varchar(max) null,
D68 varchar(max) null,
D69 varchar(max) null,
D70 varchar(max) null,
D71 varchar(max) null,
D72 varchar(max) null,
D73 varchar(max) null,
D74 varchar(max) null,
D75 varchar(max) null,
D76 varchar(max) null,
D77 varchar(max) null,
D78 varchar(max) null,
D79 varchar(max) null,
D80 varchar(max) null,
D81 varchar(max) null,
D82 varchar(max) null,
D83 varchar(max) null,
D84 varchar(max) null,
D85 varchar(max) null,
D86 varchar(max) null,
D87 varchar(max) null,
D88 varchar(max) null,
D89 varchar(max) null,
D90 varchar(max) null,
D91 varchar(max) null,
D92 varchar(max) null,
D93 varchar(max) null,
D94 varchar(max) null,
D95 varchar(max) null,
D96 varchar(max) null,
D97 varchar(max) null,
D98 varchar(max) null,
D99 varchar(max) null,
D100 varchar(max) null,
D101 varchar(max) null,
D102 varchar(max) null,
D103 varchar(max) null,
D104 varchar(max) null,
D105 varchar(max) null,
D106 varchar(max) null,
D107 varchar(max) null,
D108 varchar(max) null,
D109 varchar(max) null,
D110 varchar(max) null,
D111 varchar(max) null,
D112 varchar(max) null,
D113 varchar(max) null,
D114 varchar(max) null,
D115 varchar(max) null,
D116 varchar(max) null,
D117 varchar(max) null,
D118 varchar(max) null,
D119 varchar(max) null,
D120 varchar(max) null,
D121 varchar(max) null,
D122 varchar(max) null,
D123 varchar(max) null,
D124 varchar(max) null,
D125 varchar(max) null,
D126 varchar(max) null,
D127 varchar(max) null,
D128 varchar(max) null,
D129 varchar(max) null,
D130 varchar(max) null,
D131 varchar(max) null,
D132 varchar(max) null,
D133 varchar(max) null,
D134 varchar(max) null,
D135 varchar(max) null,
D136 varchar(max) null,
D137 varchar(max) null,
D138 varchar(max) null,
D139 varchar(max) null,
D140 varchar(max) null,
D141 varchar(max) null,
D142 varchar(max) null,
D143 varchar(max) null,
D144 varchar(max) null,
D145 varchar(max) null,
D146 varchar(max) null,
D147 varchar(max) null,
D148 varchar(max) null,
D149 varchar(max) null,
D150 varchar(max) null,
D151 varchar(max) null,
D152 varchar(max) null,
D153 varchar(max) null,
D154 varchar(max) null,
D155 varchar(max) null,
D156 varchar(max) null,
D157 varchar(max) null,
D158 varchar(max) null,
D159 varchar(max) null,
D160 varchar(max) null,
D161 varchar(max) null,
D162 varchar(max) null,
D163 varchar(max) null,
D164 varchar(max) null,
D165 varchar(max) null,
D166 varchar(max) null,
D167 varchar(max) null,
D168 varchar(max) null,
D169 varchar(max) null,
D170 varchar(max) null,
D171 varchar(max) null,
D172 varchar(max) null,
D173 varchar(max) null,
D174 varchar(max) null,
D175 varchar(max) null,
D176 varchar(max) null,
D177 varchar(max) null,
D178 varchar(max) null,
D179 varchar(max) null,
D180 varchar(max) null,
D181 varchar(max) null,
D182 varchar(max) null,
D183 varchar(max) null,
D184 varchar(max) null,
D185 varchar(max) null,
D186 varchar(max) null,
D187 varchar(max) null,
D188 varchar(max) null,
D189 varchar(max) null,
D190 varchar(max) null,
D191 varchar(max) null,
D192 varchar(max) null,
D193 varchar(max) null,
D194 varchar(max) null,
D195 varchar(max) null,
D196 varchar(max) null,
D197 varchar(max) null,
D198 varchar(max) null,
D199 varchar(max) null,
D200 varchar(max) null,
D201 varchar(max) null,
D202 varchar(max) null,
D203 varchar(max) null,
D204 varchar(max) null,
D205 varchar(max) null,
D206 varchar(max) null,
D207 varchar(max) null,
D208 varchar(max) null,
D209 varchar(max) null,
D210 varchar(max) null,
D211 varchar(max) null,
D212 varchar(max) null,
D213 varchar(max) null,
D214 varchar(max) null,
D215 varchar(max) null,
D216 varchar(max) null,
D217 varchar(max) null,
D218 varchar(max) null,
D219 varchar(max) null,
D220 varchar(max) null,



CreationDate datetime not null default getdate(),
INDEX IX_TD_PN NONCLUSTERED (ProcessName),
INDEX IX_TD_NM NONCLUSTERED (Name),
INDEX IX_TD_CD NONCLUSTERED (Code),
INDEX IX_DT_EX NONCLUSTERED (ExternalId),
INDEX IX_DT_SE NONCLUSTERED (SecondaryId ),
INDEX IX_DT_01 NONCLUSTERED (IX1),
INDEX IX_DT_02 NONCLUSTERED (IX2),
INDEX IX_DT_03 NONCLUSTERED (IX3),
)
go

select 'OK'