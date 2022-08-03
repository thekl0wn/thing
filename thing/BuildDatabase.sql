-- setup --
SET NOCOUNT ON
GO
PRINT 'switching to master...'
USE [master]
GO
IF EXISTS ( SELECT * FROM [sys].[databases] WHERE [name] = 'Thing_db' ) BEGIN
	PRINT 'removing existing database...'
	DROP DATABASE Thing_db
END
GO
PRINT 'creating new database...'
CREATE DATABASE [Thing_db]
GO
PRINT 'switching to thing_db...'
USE [Thing_db]
GO
PRINT 'creating thing schema...'
GO
CREATE SCHEMA thing
GO

-- create tables --
PRINT 'creating tables...'
PRINT '	+ [thing].[Master]'
CREATE TABLE [thing].[Master]								(
		[Id]		UNIQUEIDENTIFIER	NOT NULL			,
		[TypeId]	UNIQUEIDENTIFIER	NOT NULL			,
		[StatusId]	UNIQUEIDENTIFIER	NOT NULL			,
		[RepositoryId] UNIQUEIDENTIFIER	NOT NULL			
	CONSTRAINT pk_ThingMaster PRIMARY KEY ( [Id] )			)
GO
INSERT [thing].[Master] ( [Id], [TypeId], [StatusId], [RepositoryId] ) SELECT '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000'
GO
ALTER TABLE [thing].[Master] ADD CONSTRAINT fk_ThingMaster_Type   FOREIGN KEY ( [TypeId] )       REFERENCES [thing].[Master] ( [Id] )
ALTER TABLE [thing].[Master] ADD CONSTRAINT fk_ThingMaster_Status FOREIGN KEY ( [StatusId] )     REFERENCES [thing].[Master] ( [Id] )
ALTER TABLE [thing].[Master] ADD CONSTRAINT fk_ThingMaster_Repo   FOREIGN KEY ( [RepositoryId] ) REFERENCES [thing].[Master] ( [Id] )
GO

PRINT '	+ [thing].[Known]'
CREATE TABLE [thing].[Known]								(
		[TypeCode]	varchar(32)			NOT NULL			,
		[ThingCode]	varchar(32)			NOT NULL			,
		[Id]		UNIQUEIDENTIFIER	NOT NULL			
	CONSTRAINT pk_ThingKnown PRIMARY KEY ( [TypeCode], [ThingCode] ),
	CONSTRAINT fk_ThingKnown_Thing FOREIGN KEY ( [Id] ) REFERENCES [thing].[Master] ( [Id] ) )
GO
INSERT [thing].[Known] ( [TypeCode], [ThingCode], [Id] ) SELECT 'NULL', 'NULL', '00000000-0000-0000-0000-000000000000'
GO

PRINT '	+ [thing].[Property]'
CREATE TABLE [thing].[Property]								(
		[ThingId]	UNIQUEIDENTIFIER	NOT NULL			,
		[PropertyId] UNIQUEIDENTIFIER	NOT NULL			,
		[Value]		varchar(max)			NULL	DEFAULT''
	CONSTRAINT pk_ThingProperty PRIMARY KEY ( [ThingId], [PropertyId] ) )
GO

PRINT 'inserting default values...'
DECLARE	@NULL		UNIQUEIDENTIFIER			,
		@TYPE_PROP	UNIQUEIDENTIFIER = NEWID( )	,
		@TYPE_TYPE	UNIQUEIDENTIFIER = NEWID( )	,
		@TYPE_REPO  UNIQUEIDENTIFIER = NEWID( ) ,
		@TYPE_STS	UNIQUEIDENTIFIER = NEWID( )	,
		@STS_ACTIVE	UNIQUEIDENTIFIER = NEWID( )	,
		@STS_DEACTI	UNIQUEIDENTIFIER = NEWID( ) ,
		@PROP_NAME	UNIQUEIDENTIFIER = NEWID( )	,
		@PROP_CODE  UNIQUEIDENTIFIER = NEWID( ) ,
		@PROP_DESC	UNIQUEIDENTIFIER = NEWID( ) ,
		@REPO_SYS   UNIQUEIDENTIFIER = NEWID( ) ,
		@REPO_NEW   UNIQUEIDENTIFIER = NEWID( )	
SET		@NULL = '00000000-0000-0000-0000-000000000000'

-- insert the initial, bare minimum things --
INSERT [thing].[Master] ( [Id], [TypeId], [StatusId], [RepositoryId] )
	  SELECT @TYPE_TYPE, @NULL, @NULL, @NULL
UNION SELECT @TYPE_PROP, @NULL, @NULL, @NULL
UNION SELECT @TYPE_STS,  @NULL, @NULL, @NULL
UNION SELECT @TYPE_REPO, @NULL, @NULL, @NULL
UPDATE [thing].[Master] SET [TypeId] = @TYPE_TYPE WHERE [Id] IN ( @TYPE_TYPE, @TYPE_PROP, @TYPE_STS, @TYPE_REPO )

-- insert the active status and set existing things to active --
INSERT [thing].[Master] ( [Id], [TypeId], [StatusId], [RepositoryId] )
	  SELECT @STS_ACTIVE, @TYPE_STS, @NULL, @NULL
UPDATE [thing].[Master] SET [StatusId] = @STS_ACTIVE WHERE [Id] <> @NULL

-- insert the system repository and set active objects --
INSERT [thing].[Master] ( [Id], [TypeId], [StatusId], [RepositoryId] )
      SELECT @REPO_SYS, @TYPE_REPO, @STS_ACTIVE, @NULL
UPDATE [thing].[Master] SET [RepositoryId] = @REPO_SYS WHERE [Id] <> @NULL

-- start inserting properties --
INSERT [thing].[Master] ( [Id], [TypeId], [StatusId], [RepositoryId] )
      SELECT @REPO_NEW,   @TYPE_REPO, @STS_ACTIVE, @REPO_SYS
UNION SELECT @STS_DEACTI, @TYPE_STS,  @STS_ACTIVE, @REPO_SYS
UNION SELECT @PROP_CODE,  @TYPE_PROP, @STS_ACTIVE, @REPO_SYS
UNION SELECT @PROP_NAME,  @TYPE_PROP, @STS_ACTIVE, @REPO_SYS
UNION SELECT @PROP_DESC,  @TYPE_PROP, @STS_ACTIVE, @REPO_SYS

-- set known id's --
INSERT [thing].[Known] ( [TypeCode], [ThingCode], [Id] )
	  SELECT 'NULL',       'NULL',        @NULL
UNION SELECT 'TYPE',       'TYPE',        @TYPE_TYPE
UNION SELECT 'TYPE',       'PROPERTY',    @TYPE_PROP
UNION SELECT 'TYPE',       'STATUS',      @TYPE_STS
UNION SELECT 'TYPE',       'REPOSITORY',  @TYPE_REPO
UNION SELECT 'STATUS',     'ACTIVE',      @STS_ACTIVE
UNION SELECT 'STATUS',     'DEACTIVATED', @STS_DEACTI
UNION SELECT 'PROPERTY',   'NAME',        @PROP_NAME
UNION SELECT 'PROPERTY',   'CODE',        @PROP_CODE
UNION SELECT 'PROPERTY',   'DESCRIPTION', @PROP_DESC
UNION SELECT 'REPOSITORY', 'SYSTEM',      @REPO_SYS
UNION SELECT 'REPOSITORY', 'NEW',         @REPO_NEW

-- set names --
INSERT [thing].[Property] ( [ThingId], [PropertyId], [Value] )
      SELECT @NULL,       @PROP_NAME, '<NULL>'

UNION SELECT @TYPE_TYPE,  @PROP_NAME, 'Type'
UNION SELECT @TYPE_PROP,  @PROP_NAME, 'Property'
UNION SELECT @TYPE_REPO,  @PROP_NAME, 'Repository'
UNION SELECT @TYPE_STS,   @PROP_NAME, 'Status'

UNION SELECT @PROP_DESC,  @PROP_NAME, 'Description'
UNION SELECT @PROP_NAME,  @PROP_NAME, 'Name'
UNION SELECT @PROP_CODE,  @PROP_NAME, 'Code'

UNION SELECT @STS_ACTIVE, @PROP_NAME, 'Active'
UNION SELECT @STS_DEACTI, @PROP_NAME, 'Deactivated'

UNION SELECT @REPO_SYS, @PROP_NAME, 'System'
UNION SELECT @REPO_NEW, @PROP_NAME, 'New Buffer'

-- set codes --
UNION SELECT @NULL,       @PROP_CODE, 'NULL'

UNION SELECT @TYPE_TYPE,  @PROP_CODE, 'TYPE'
UNION SELECT @TYPE_PROP,  @PROP_CODE, 'PROPERTY'
UNION SELECT @TYPE_REPO,  @PROP_CODE, 'REPOSITORY'
UNION SELECT @TYPE_STS,   @PROP_CODE, 'STATUS'

UNION SELECT @PROP_DESC,  @PROP_CODE, 'DESCRIPTION'
UNION SELECT @PROP_NAME,  @PROP_CODE, 'NAME'
UNION SELECT @PROP_CODE,  @PROP_CODE, 'CODE'

UNION SELECT @STS_ACTIVE, @PROP_CODE, 'ACTIVE'
UNION SELECT @STS_DEACTI, @PROP_CODE, 'DEACTIVATED'

UNION SELECT @REPO_SYS, @PROP_CODE, 'SYSTEM'
UNION SELECT @REPO_NEW, @PROP_CODE, 'NEW'
GO

SELECT * FROM [thing].[Property]

		