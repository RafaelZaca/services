IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Groups] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Groups] PRIMARY KEY ([Id])
);

CREATE TABLE [OperationalMetrics] (
    [Id] int NOT NULL IDENTITY,
    [Date] datetime2 NOT NULL,
    [ProcessedBoxes] int NOT NULL,
    [RejectedBoxes] int NOT NULL,
    [OperatingHours] decimal(18,2) NOT NULL,
    [StoppedHours] decimal(18,2) NOT NULL,
    [CompletedWaves] int NOT NULL,
    [CompletedShipments] int NOT NULL,
    [SystemAvailability] decimal(18,2) NOT NULL,
    [AverageWaveCompletionTime] decimal(18,2) NOT NULL,
    [ShiftId] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_OperationalMetrics] PRIMARY KEY ([Id])
);

CREATE TABLE [Permissions] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Resource] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);

CREATE TABLE [PickRequests] (
    [Id] nvarchar(450) NOT NULL,
    [OrderNumber] nvarchar(max) NOT NULL,
    [StopId] nvarchar(max) NOT NULL,
    [DeliveryType] nvarchar(max) NOT NULL,
    [ClientName] nvarchar(max) NOT NULL,
    [Customization] nvarchar(max) NOT NULL,
    [TotalQuantity] int NOT NULL,
    [TotalSkus] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    CONSTRAINT [PK_PickRequests] PRIMARY KEY ([Id])
);

CREATE TABLE [ProductionHourlies] (
    [Id] int NOT NULL IDENTITY,
    [Hour] datetime2 NOT NULL,
    [PlannedQuantity] int NOT NULL,
    [ProducedQuantity] int NOT NULL,
    [RejectedQuantity] int NOT NULL,
    [Capacity] int NOT NULL,
    [WaveNumber] nvarchar(max) NOT NULL,
    [EfficiencyRate] decimal(18,2) NOT NULL,
    [RejectionRate] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ProductionHourlies] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [GroupId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLogin] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Groups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [Groups] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [OperationalMetricsDetail] (
    [Id] int NOT NULL IDENTITY,
    [OperationalMetricsId] int NOT NULL,
    [StartTime] datetime2 NOT NULL,
    [EndTime] datetime2 NOT NULL,
    [ProcessedBoxes] int NOT NULL,
    [RejectedBoxes] int NOT NULL,
    [StopReason] nvarchar(max) NOT NULL,
    [ProductivityRate] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_OperationalMetricsDetail] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OperationalMetricsDetail_OperationalMetrics_OperationalMetricsId] FOREIGN KEY ([OperationalMetricsId]) REFERENCES [OperationalMetrics] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [GroupPermission] (
    [GroupsId] int NOT NULL,
    [PermissionsId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_GroupPermission] PRIMARY KEY ([GroupsId], [PermissionsId]),
    CONSTRAINT [FK_GroupPermission_Groups_GroupsId] FOREIGN KEY ([GroupsId]) REFERENCES [Groups] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupPermission_Permissions_PermissionsId] FOREIGN KEY ([PermissionsId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [GroupPermissions] (
    [GroupId] int NOT NULL,
    [PermissionId] nvarchar(450) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_GroupPermissions] PRIMARY KEY ([GroupId], [PermissionId]),
    CONSTRAINT [FK_GroupPermissions_Groups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [Groups] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupPermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PickRequestItems] (
    [Id] int NOT NULL IDENTITY,
    [PickRequestId] nvarchar(450) NOT NULL,
    [Sku] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [ExpectedQuantity] int NOT NULL,
    [InducedQuantity] int NOT NULL,
    [ReceivedQuantity] int NOT NULL,
    [InductionPercentage] decimal(18,2) NOT NULL,
    [ReceiptPercentage] decimal(18,2) NOT NULL,
    [BatchNumber] nvarchar(max) NOT NULL,
    [NoReadRejectionCount] int NOT NULL,
    [FinalRejectionCount] int NOT NULL,
    [PendingCount] int NOT NULL,
    [LastBoxReceivedAt] datetime2 NULL,
    CONSTRAINT [PK_PickRequestItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PickRequestItems_PickRequests_PickRequestId] FOREIGN KEY ([PickRequestId]) REFERENCES [PickRequests] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Ramps] (
    [Id] int NOT NULL IDENTITY,
    [RampNumber] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [CurrentPickRequestId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_Ramps] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ramps_PickRequests_CurrentPickRequestId] FOREIGN KEY ([CurrentPickRequestId]) REFERENCES [PickRequests] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ClientCustomizations] (
    [Id] int NOT NULL IDENTITY,
    [ClientName] nvarchar(max) NOT NULL,
    [CustomizationType] nvarchar(max) NOT NULL,
    [CustomizationRule] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedByUserId] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedByUserId] int NULL,
    CONSTRAINT [PK_ClientCustomizations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientCustomizations_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ClientCustomizations_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [SkuBatches] (
    [Id] int NOT NULL IDENTITY,
    [Sku] nvarchar(max) NOT NULL,
    [BatchNumber] nvarchar(max) NOT NULL,
    [RegisteredAt] datetime2 NOT NULL,
    [RegisteredByUserId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [WaveNumber] nvarchar(max) NOT NULL,
    [FirstUsedAt] datetime2 NULL,
    [LastUsedAt] datetime2 NULL,
    CONSTRAINT [PK_SkuBatches] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SkuBatches_Users_RegisteredByUserId] FOREIGN KEY ([RegisteredByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SystemAlerts] (
    [Id] int NOT NULL IDENTITY,
    [AlertType] nvarchar(max) NOT NULL,
    [Severity] nvarchar(max) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [Parameters] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [RequiresAcknowledgment] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [AcknowledgedAt] datetime2 NULL,
    [AcknowledgedByUserId] int NULL,
    CONSTRAINT [PK_SystemAlerts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SystemAlerts_Users_AcknowledgedByUserId] FOREIGN KEY ([AcknowledgedByUserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [SystemParameters] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(max) NOT NULL,
    [Value] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [DataType] nvarchar(max) NOT NULL,
    [IsEditable] bit NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedByUserId] int NULL,
    CONSTRAINT [PK_SystemParameters] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SystemParameters_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [Waves] (
    [Id] int NOT NULL IDENTITY,
    [WaveNumber] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [StartedAt] datetime2 NULL,
    [CompletedAt] datetime2 NULL,
    [CreatedByUserId] int NOT NULL,
    CONSTRAINT [PK_Waves] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Waves_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ProductionHourlyDetails] (
    [Id] int NOT NULL IDENTITY,
    [ProductionHourlyId] int NOT NULL,
    [RampId] int NOT NULL,
    [ProducedQuantity] int NOT NULL,
    [RejectedQuantity] int NOT NULL,
    [EfficiencyRate] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ProductionHourlyDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductionHourlyDetails_ProductionHourlies_ProductionHourlyId] FOREIGN KEY ([ProductionHourlyId]) REFERENCES [ProductionHourlies] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductionHourlyDetails_Ramps_RampId] FOREIGN KEY ([RampId]) REFERENCES [Ramps] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RampAllocations] (
    [Id] int NOT NULL IDENTITY,
    [RampId] int NOT NULL,
    [PickRequestId] nvarchar(450) NOT NULL,
    [AllocatedAt] datetime2 NOT NULL,
    [ReleasedAt] datetime2 NULL,
    [AllocatedByUserId] int NOT NULL,
    [ReleasedByUserId] int NULL,
    CONSTRAINT [PK_RampAllocations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RampAllocations_PickRequests_PickRequestId] FOREIGN KEY ([PickRequestId]) REFERENCES [PickRequests] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RampAllocations_Ramps_RampId] FOREIGN KEY ([RampId]) REFERENCES [Ramps] ([Id]),
    CONSTRAINT [FK_RampAllocations_Users_AllocatedByUserId] FOREIGN KEY ([AllocatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RampAllocations_Users_ReleasedByUserId] FOREIGN KEY ([ReleasedByUserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [RampConfigurations] (
    [Id] int NOT NULL IDENTITY,
    [RampId] int NOT NULL,
    [MaximumBoxes] int NOT NULL,
    [WarningThreshold] int NOT NULL,
    [AutomaticRelease] bit NOT NULL,
    [MaxWaitTime] int NOT NULL,
    [ValidCustomizations] nvarchar(max) NOT NULL,
    [SpecialInstructions] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedByUserId] int NOT NULL,
    CONSTRAINT [PK_RampConfigurations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RampConfigurations_Ramps_RampId] FOREIGN KEY ([RampId]) REFERENCES [Ramps] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RampConfigurations_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RampMetrics] (
    [Id] int NOT NULL IDENTITY,
    [RampId] int NOT NULL,
    [Date] datetime2 NOT NULL,
    [TotalPickRequests] int NOT NULL,
    [TotalBoxes] int NOT NULL,
    [TotalSkus] int NOT NULL,
    [AverageProcessingTime] decimal(18,2) NOT NULL,
    [UtilizationRate] decimal(18,2) NOT NULL,
    [NoReadRejections] int NOT NULL,
    [ExcessRejections] int NOT NULL,
    [FullRampRejections] int NOT NULL,
    CONSTRAINT [PK_RampMetrics] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RampMetrics_Ramps_RampId] FOREIGN KEY ([RampId]) REFERENCES [Ramps] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Rejections] (
    [Id] int NOT NULL IDENTITY,
    [PickRequestId] nvarchar(450) NOT NULL,
    [Sku] nvarchar(max) NOT NULL,
    [RejectionType] nvarchar(max) NOT NULL,
    [RampId] int NOT NULL,
    [RejectedAt] datetime2 NOT NULL,
    [IsTreated] bit NOT NULL,
    [TreatedAt] datetime2 NULL,
    [TreatedByUserId] int NULL,
    [BatchNumber] nvarchar(max) NOT NULL,
    [TreatmentNotes] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Rejections] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Rejections_PickRequests_PickRequestId] FOREIGN KEY ([PickRequestId]) REFERENCES [PickRequests] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Rejections_Ramps_RampId] FOREIGN KEY ([RampId]) REFERENCES [Ramps] ([Id]),
    CONSTRAINT [FK_Rejections_Users_TreatedByUserId] FOREIGN KEY ([TreatedByUserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [SkuBatchUsages] (
    [Id] int NOT NULL IDENTITY,
    [SkuBatchId] int NOT NULL,
    [PickRequestId] nvarchar(450) NOT NULL,
    [RampId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UsedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SkuBatchUsages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SkuBatchUsages_PickRequests_PickRequestId] FOREIGN KEY ([PickRequestId]) REFERENCES [PickRequests] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SkuBatchUsages_Ramps_RampId] FOREIGN KEY ([RampId]) REFERENCES [Ramps] ([Id]),
    CONSTRAINT [FK_SkuBatchUsages_SkuBatches_SkuBatchId] FOREIGN KEY ([SkuBatchId]) REFERENCES [SkuBatches] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [WavePickRequests] (
    [WaveId] int NOT NULL,
    [PickRequestId] nvarchar(450) NOT NULL,
    [AddedAt] datetime2 NOT NULL,
    [AddedByUserId] int NOT NULL,
    [PickRequestId1] nvarchar(450) NULL,
    CONSTRAINT [PK_WavePickRequests] PRIMARY KEY ([WaveId], [PickRequestId]),
    CONSTRAINT [FK_WavePickRequests_PickRequests_PickRequestId] FOREIGN KEY ([PickRequestId]) REFERENCES [PickRequests] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WavePickRequests_PickRequests_PickRequestId1] FOREIGN KEY ([PickRequestId1]) REFERENCES [PickRequests] ([Id]),
    CONSTRAINT [FK_WavePickRequests_Users_AddedByUserId] FOREIGN KEY ([AddedByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WavePickRequests_Waves_WaveId] FOREIGN KEY ([WaveId]) REFERENCES [Waves] ([Id])
);

CREATE TABLE [ManualTriages] (
    [Id] int NOT NULL IDENTITY,
    [Sku] nvarchar(max) NOT NULL,
    [DestinationRampId] int NOT NULL,
    [BatchNumber] nvarchar(max) NOT NULL,
    [Quantity] int NOT NULL,
    [TriagedAt] datetime2 NOT NULL,
    [TriagedByUserId] int NOT NULL,
    [PickRequestId] nvarchar(450) NOT NULL,
    [RejectionId] int NULL,
    CONSTRAINT [PK_ManualTriages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ManualTriages_PickRequests_PickRequestId] FOREIGN KEY ([PickRequestId]) REFERENCES [PickRequests] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ManualTriages_Ramps_DestinationRampId] FOREIGN KEY ([DestinationRampId]) REFERENCES [Ramps] ([Id]),
    CONSTRAINT [FK_ManualTriages_Rejections_RejectionId] FOREIGN KEY ([RejectionId]) REFERENCES [Rejections] ([Id]),
    CONSTRAINT [FK_ManualTriages_Users_TriagedByUserId] FOREIGN KEY ([TriagedByUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_ClientCustomizations_CreatedByUserId] ON [ClientCustomizations] ([CreatedByUserId]);

CREATE INDEX [IX_ClientCustomizations_UpdatedByUserId] ON [ClientCustomizations] ([UpdatedByUserId]);

CREATE INDEX [IX_GroupPermission_PermissionsId] ON [GroupPermission] ([PermissionsId]);

CREATE INDEX [IX_GroupPermissions_PermissionId] ON [GroupPermissions] ([PermissionId]);

CREATE INDEX [IX_ManualTriages_DestinationRampId] ON [ManualTriages] ([DestinationRampId]);

CREATE INDEX [IX_ManualTriages_PickRequestId] ON [ManualTriages] ([PickRequestId]);

CREATE INDEX [IX_ManualTriages_RejectionId] ON [ManualTriages] ([RejectionId]);

CREATE INDEX [IX_ManualTriages_TriagedByUserId] ON [ManualTriages] ([TriagedByUserId]);

CREATE INDEX [IX_OperationalMetricsDetail_OperationalMetricsId] ON [OperationalMetricsDetail] ([OperationalMetricsId]);

CREATE INDEX [IX_PickRequestItems_PickRequestId] ON [PickRequestItems] ([PickRequestId]);

CREATE INDEX [IX_ProductionHourlyDetails_ProductionHourlyId] ON [ProductionHourlyDetails] ([ProductionHourlyId]);

CREATE INDEX [IX_ProductionHourlyDetails_RampId] ON [ProductionHourlyDetails] ([RampId]);

CREATE INDEX [IX_RampAllocations_AllocatedByUserId] ON [RampAllocations] ([AllocatedByUserId]);

CREATE INDEX [IX_RampAllocations_PickRequestId] ON [RampAllocations] ([PickRequestId]);

CREATE INDEX [IX_RampAllocations_RampId] ON [RampAllocations] ([RampId]);

CREATE INDEX [IX_RampAllocations_ReleasedByUserId] ON [RampAllocations] ([ReleasedByUserId]);

CREATE INDEX [IX_RampConfigurations_RampId] ON [RampConfigurations] ([RampId]);

CREATE INDEX [IX_RampConfigurations_UpdatedByUserId] ON [RampConfigurations] ([UpdatedByUserId]);

CREATE INDEX [IX_RampMetrics_RampId] ON [RampMetrics] ([RampId]);

CREATE INDEX [IX_Ramps_CurrentPickRequestId] ON [Ramps] ([CurrentPickRequestId]);

CREATE INDEX [IX_Rejections_PickRequestId] ON [Rejections] ([PickRequestId]);

CREATE INDEX [IX_Rejections_RampId] ON [Rejections] ([RampId]);

CREATE INDEX [IX_Rejections_TreatedByUserId] ON [Rejections] ([TreatedByUserId]);

CREATE INDEX [IX_SkuBatches_RegisteredByUserId] ON [SkuBatches] ([RegisteredByUserId]);

CREATE INDEX [IX_SkuBatchUsages_PickRequestId] ON [SkuBatchUsages] ([PickRequestId]);

CREATE INDEX [IX_SkuBatchUsages_RampId] ON [SkuBatchUsages] ([RampId]);

CREATE INDEX [IX_SkuBatchUsages_SkuBatchId] ON [SkuBatchUsages] ([SkuBatchId]);

CREATE INDEX [IX_SystemAlerts_AcknowledgedByUserId] ON [SystemAlerts] ([AcknowledgedByUserId]);

CREATE INDEX [IX_SystemParameters_UpdatedByUserId] ON [SystemParameters] ([UpdatedByUserId]);

CREATE INDEX [IX_Users_GroupId] ON [Users] ([GroupId]);

CREATE INDEX [IX_WavePickRequests_AddedByUserId] ON [WavePickRequests] ([AddedByUserId]);

CREATE INDEX [IX_WavePickRequests_PickRequestId] ON [WavePickRequests] ([PickRequestId]);

CREATE INDEX [IX_WavePickRequests_PickRequestId1] ON [WavePickRequests] ([PickRequestId1]);

CREATE INDEX [IX_Waves_CreatedByUserId] ON [Waves] ([CreatedByUserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250217005946_Rafael_001', N'9.0.2');

ALTER TABLE [Ramps] DROP CONSTRAINT [FK_Ramps_PickRequests_CurrentPickRequestId];

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ramps]') AND [c].[name] = N'CurrentPickRequestId');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Ramps] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Ramps] ALTER COLUMN [CurrentPickRequestId] nvarchar(450) NULL;

ALTER TABLE [Ramps] ADD CONSTRAINT [FK_Ramps_PickRequests_CurrentPickRequestId] FOREIGN KEY ([CurrentPickRequestId]) REFERENCES [PickRequests] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250309123947_Rafael_002', N'9.0.2');

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'ReceivedQuantity');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [ReceivedQuantity] int NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'ReceiptPercentage');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [ReceiptPercentage] decimal(18,2) NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'PendingCount');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [PendingCount] int NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'NoReadRejectionCount');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [NoReadRejectionCount] int NULL;

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'InductionPercentage');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [InductionPercentage] decimal(18,2) NULL;

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'InducedQuantity');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [InducedQuantity] int NULL;

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'FinalRejectionCount');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [FinalRejectionCount] int NULL;

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'ExpectedQuantity');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [ExpectedQuantity] int NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250309125021_Rafael_003', N'9.0.2');

DROP TABLE [GroupPermission];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250309141829_Rafael_004', N'9.0.2');

ALTER TABLE [PickRequestItems] ADD [AssetType] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [PickRequestItems] ADD [PackageUnit] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [PickRequestItems] ADD [SourcePallet] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250430112802_Rafael_005', N'9.0.2');

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'SourcePallet');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [SourcePallet] nvarchar(max) NULL;

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'PackageUnit');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [PackageUnit] nvarchar(max) NULL;

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PickRequestItems]') AND [c].[name] = N'AssetType');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [PickRequestItems] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [PickRequestItems] ALTER COLUMN [AssetType] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250430113921_Rafael_006', N'9.0.2');

ALTER TABLE [PickRequestItems] ADD [WorkReference] nvarchar(max) NOT NULL DEFAULT N'';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250430114108_Rafael_007', N'9.0.2');

COMMIT;
GO

