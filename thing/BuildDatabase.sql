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
		[StatusId]	UNIQUEIDENTIFIER	NOT NULL				
	CONSTRAINT pk_ThingMaster PRIMARY KEY ( [Id] )			)
GO
INSERT [thing].[Master] ( [Id], [TypeId], [StatusId] ) SELECT '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000'
GO
ALTER TABLE [thing].[Master] ADD CONSTRAINT fk_ThingMaster_Type   FOREIGN KEY ( [TypeId] )   REFERENCES [thing].[Master] ( [Id] )
ALTER TABLE [thing].[Master] ADD CONSTRAINT fk_ThingMaster_Status FOREIGN KEY ( [StatusId] ) REFERENCES [thing].[Master] ( [Id] )
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
		@TYPE_STS	UNIQUEIDENTIFIER = NEWID( )	,
		@STS_ACTIVE	UNIQUEIDENTIFIER = NEWID( )	,
		@PROP_NAME	UNIQUEIDENTIFIER = NEWID( )	,
		@PROP_DESC	UNIQUEIDENTIFIER = NEWID( )
SET		@NULL = '00000000-0000-0000-0000-000000000000'

-- insert the initial, bare minimum things --
INSERT [thing].[Master] ( [Id], [TypeId], [StatusId] )
	  SELECT @TYPE_TYPE, @NULL, @NULL
UNION SELECT @TYPE_PROP, @NULL, @NULL
UNION SELECT @TYPE_STS,  @NULL, @NULL
UPDATE [thing].[Master] SET [TypeId] = @TYPE_TYPE WHERE [Id] IN ( @TYPE_TYPE, @TYPE_PROP, @TYPE_STS )

-- insert the active status and set existing things to active --
INSERT [thing].[Master] ( [Id], [TypeId], [StatusId] )
	  SELECT @STS_ACTIVE, @TYPE_STS, @NULL
UPDATE [thing].[Master] SET [StatusId] = @STS_ACTIVE WHERE [Id] <> @NULL

-- start inserting properties --
INSERT [thing].[Master] ( [Id], [TypeId], [StatusId] )
	  SELECT @PROP_NAME, @TYPE_PROP, @STS_ACTIVE
UNION SELECT @PROP_DESC, @TYPE_PROP, @STS_ACTIVE

-- set known id's --
INSERT [thing].[Known] ( [TypeCode], [ThingCode], [Id] )
      SELECT 'TYPE',     'TYPE',        @TYPE_TYPE
UNION SELECT 'TYPE',     'PROPERTY',    @TYPE_PROP
UNION SELECT 'TYPE',     'STATUS',      @TYPE_STS
UNION SELECT 'STATUS',   'ACTIVE',      @STS_ACTIVE
UNION SELECT 'PROPERTY', 'NAME',        @PROP_NAME
UNION SELECT 'PROPERTY', 'DESCRIPTION', @PROP_DESC
GO

SELECT * FROM [thing].[Master]

		