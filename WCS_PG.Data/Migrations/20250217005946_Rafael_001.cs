using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WCS_PG.Data.Migrations
{
    /// <inheritdoc />
    public partial class Rafael_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationalMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedBoxes = table.Column<int>(type: "int", nullable: false),
                    RejectedBoxes = table.Column<int>(type: "int", nullable: false),
                    OperatingHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoppedHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompletedWaves = table.Column<int>(type: "int", nullable: false),
                    CompletedShipments = table.Column<int>(type: "int", nullable: false),
                    SystemAvailability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageWaveCompletionTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShiftId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resource = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PickRequests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StopId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Customization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalSkus = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductionHourlies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Hour = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlannedQuantity = table.Column<int>(type: "int", nullable: false),
                    ProducedQuantity = table.Column<int>(type: "int", nullable: false),
                    RejectedQuantity = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    WaveNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EfficiencyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RejectionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionHourlies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperationalMetricsDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationalMetricsId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedBoxes = table.Column<int>(type: "int", nullable: false),
                    RejectedBoxes = table.Column<int>(type: "int", nullable: false),
                    StopReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductivityRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalMetricsDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationalMetricsDetail_OperationalMetrics_OperationalMetricsId",
                        column: x => x.OperationalMetricsId,
                        principalTable: "OperationalMetrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupPermission",
                columns: table => new
                {
                    GroupsId = table.Column<int>(type: "int", nullable: false),
                    PermissionsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPermission", x => new { x.GroupsId, x.PermissionsId });
                    table.ForeignKey(
                        name: "FK_GroupPermission_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPermission_Permissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupPermissions",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPermissions", x => new { x.GroupId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_GroupPermissions_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PickRequestItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedQuantity = table.Column<int>(type: "int", nullable: false),
                    InducedQuantity = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "int", nullable: false),
                    InductionPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceiptPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoReadRejectionCount = table.Column<int>(type: "int", nullable: false),
                    FinalRejectionCount = table.Column<int>(type: "int", nullable: false),
                    PendingCount = table.Column<int>(type: "int", nullable: false),
                    LastBoxReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickRequestItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PickRequestItems_PickRequests_PickRequestId",
                        column: x => x.PickRequestId,
                        principalTable: "PickRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ramps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RampNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CurrentPickRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ramps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ramps_PickRequests_CurrentPickRequestId",
                        column: x => x.CurrentPickRequestId,
                        principalTable: "PickRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientCustomizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomizationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomizationRule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCustomizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientCustomizations_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientCustomizations_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SkuBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegisteredByUserId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WaveNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkuBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkuBatches_Users_RegisteredByUserId",
                        column: x => x.RegisteredByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RequiresAcknowledgment = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemAlerts_Users_AcknowledgedByUserId",
                        column: x => x.AcknowledgedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemParameters_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Waves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WaveNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Waves_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductionHourlyDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionHourlyId = table.Column<int>(type: "int", nullable: false),
                    RampId = table.Column<int>(type: "int", nullable: false),
                    ProducedQuantity = table.Column<int>(type: "int", nullable: false),
                    RejectedQuantity = table.Column<int>(type: "int", nullable: false),
                    EfficiencyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionHourlyDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionHourlyDetails_ProductionHourlies_ProductionHourlyId",
                        column: x => x.ProductionHourlyId,
                        principalTable: "ProductionHourlies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionHourlyDetails_Ramps_RampId",
                        column: x => x.RampId,
                        principalTable: "Ramps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RampAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RampId = table.Column<int>(type: "int", nullable: false),
                    PickRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AllocatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AllocatedByUserId = table.Column<int>(type: "int", nullable: false),
                    ReleasedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RampAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RampAllocations_PickRequests_PickRequestId",
                        column: x => x.PickRequestId,
                        principalTable: "PickRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RampAllocations_Ramps_RampId",
                        column: x => x.RampId,
                        principalTable: "Ramps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RampAllocations_Users_AllocatedByUserId",
                        column: x => x.AllocatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RampAllocations_Users_ReleasedByUserId",
                        column: x => x.ReleasedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RampConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RampId = table.Column<int>(type: "int", nullable: false),
                    MaximumBoxes = table.Column<int>(type: "int", nullable: false),
                    WarningThreshold = table.Column<int>(type: "int", nullable: false),
                    AutomaticRelease = table.Column<bool>(type: "bit", nullable: false),
                    MaxWaitTime = table.Column<int>(type: "int", nullable: false),
                    ValidCustomizations = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RampConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RampConfigurations_Ramps_RampId",
                        column: x => x.RampId,
                        principalTable: "Ramps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RampConfigurations_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RampMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RampId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPickRequests = table.Column<int>(type: "int", nullable: false),
                    TotalBoxes = table.Column<int>(type: "int", nullable: false),
                    TotalSkus = table.Column<int>(type: "int", nullable: false),
                    AverageProcessingTime = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UtilizationRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NoReadRejections = table.Column<int>(type: "int", nullable: false),
                    ExcessRejections = table.Column<int>(type: "int", nullable: false),
                    FullRampRejections = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RampMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RampMetrics_Ramps_RampId",
                        column: x => x.RampId,
                        principalTable: "Ramps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rejections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RampId = table.Column<int>(type: "int", nullable: false),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsTreated = table.Column<bool>(type: "bit", nullable: false),
                    TreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreatmentNotes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rejections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rejections_PickRequests_PickRequestId",
                        column: x => x.PickRequestId,
                        principalTable: "PickRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rejections_Ramps_RampId",
                        column: x => x.RampId,
                        principalTable: "Ramps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rejections_Users_TreatedByUserId",
                        column: x => x.TreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SkuBatchUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkuBatchId = table.Column<int>(type: "int", nullable: false),
                    PickRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RampId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkuBatchUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkuBatchUsages_PickRequests_PickRequestId",
                        column: x => x.PickRequestId,
                        principalTable: "PickRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkuBatchUsages_Ramps_RampId",
                        column: x => x.RampId,
                        principalTable: "Ramps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SkuBatchUsages_SkuBatches_SkuBatchId",
                        column: x => x.SkuBatchId,
                        principalTable: "SkuBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WavePickRequests",
                columns: table => new
                {
                    WaveId = table.Column<int>(type: "int", nullable: false),
                    PickRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddedByUserId = table.Column<int>(type: "int", nullable: false),
                    PickRequestId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WavePickRequests", x => new { x.WaveId, x.PickRequestId });
                    table.ForeignKey(
                        name: "FK_WavePickRequests_PickRequests_PickRequestId",
                        column: x => x.PickRequestId,
                        principalTable: "PickRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WavePickRequests_PickRequests_PickRequestId1",
                        column: x => x.PickRequestId1,
                        principalTable: "PickRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WavePickRequests_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WavePickRequests_Waves_WaveId",
                        column: x => x.WaveId,
                        principalTable: "Waves",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ManualTriages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationRampId = table.Column<int>(type: "int", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TriagedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TriagedByUserId = table.Column<int>(type: "int", nullable: false),
                    PickRequestId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RejectionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualTriages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManualTriages_PickRequests_PickRequestId",
                        column: x => x.PickRequestId,
                        principalTable: "PickRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManualTriages_Ramps_DestinationRampId",
                        column: x => x.DestinationRampId,
                        principalTable: "Ramps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ManualTriages_Rejections_RejectionId",
                        column: x => x.RejectionId,
                        principalTable: "Rejections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ManualTriages_Users_TriagedByUserId",
                        column: x => x.TriagedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientCustomizations_CreatedByUserId",
                table: "ClientCustomizations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCustomizations_UpdatedByUserId",
                table: "ClientCustomizations",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermission_PermissionsId",
                table: "GroupPermission",
                column: "PermissionsId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissions_PermissionId",
                table: "GroupPermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualTriages_DestinationRampId",
                table: "ManualTriages",
                column: "DestinationRampId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualTriages_PickRequestId",
                table: "ManualTriages",
                column: "PickRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualTriages_RejectionId",
                table: "ManualTriages",
                column: "RejectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ManualTriages_TriagedByUserId",
                table: "ManualTriages",
                column: "TriagedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalMetricsDetail_OperationalMetricsId",
                table: "OperationalMetricsDetail",
                column: "OperationalMetricsId");

            migrationBuilder.CreateIndex(
                name: "IX_PickRequestItems_PickRequestId",
                table: "PickRequestItems",
                column: "PickRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHourlyDetails_ProductionHourlyId",
                table: "ProductionHourlyDetails",
                column: "ProductionHourlyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHourlyDetails_RampId",
                table: "ProductionHourlyDetails",
                column: "RampId");

            migrationBuilder.CreateIndex(
                name: "IX_RampAllocations_AllocatedByUserId",
                table: "RampAllocations",
                column: "AllocatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RampAllocations_PickRequestId",
                table: "RampAllocations",
                column: "PickRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RampAllocations_RampId",
                table: "RampAllocations",
                column: "RampId");

            migrationBuilder.CreateIndex(
                name: "IX_RampAllocations_ReleasedByUserId",
                table: "RampAllocations",
                column: "ReleasedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RampConfigurations_RampId",
                table: "RampConfigurations",
                column: "RampId");

            migrationBuilder.CreateIndex(
                name: "IX_RampConfigurations_UpdatedByUserId",
                table: "RampConfigurations",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RampMetrics_RampId",
                table: "RampMetrics",
                column: "RampId");

            migrationBuilder.CreateIndex(
                name: "IX_Ramps_CurrentPickRequestId",
                table: "Ramps",
                column: "CurrentPickRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Rejections_PickRequestId",
                table: "Rejections",
                column: "PickRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Rejections_RampId",
                table: "Rejections",
                column: "RampId");

            migrationBuilder.CreateIndex(
                name: "IX_Rejections_TreatedByUserId",
                table: "Rejections",
                column: "TreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SkuBatches_RegisteredByUserId",
                table: "SkuBatches",
                column: "RegisteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SkuBatchUsages_PickRequestId",
                table: "SkuBatchUsages",
                column: "PickRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SkuBatchUsages_RampId",
                table: "SkuBatchUsages",
                column: "RampId");

            migrationBuilder.CreateIndex(
                name: "IX_SkuBatchUsages_SkuBatchId",
                table: "SkuBatchUsages",
                column: "SkuBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemAlerts_AcknowledgedByUserId",
                table: "SystemAlerts",
                column: "AcknowledgedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemParameters_UpdatedByUserId",
                table: "SystemParameters",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupId",
                table: "Users",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WavePickRequests_AddedByUserId",
                table: "WavePickRequests",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WavePickRequests_PickRequestId",
                table: "WavePickRequests",
                column: "PickRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WavePickRequests_PickRequestId1",
                table: "WavePickRequests",
                column: "PickRequestId1");

            migrationBuilder.CreateIndex(
                name: "IX_Waves_CreatedByUserId",
                table: "Waves",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientCustomizations");

            migrationBuilder.DropTable(
                name: "GroupPermission");

            migrationBuilder.DropTable(
                name: "GroupPermissions");

            migrationBuilder.DropTable(
                name: "ManualTriages");

            migrationBuilder.DropTable(
                name: "OperationalMetricsDetail");

            migrationBuilder.DropTable(
                name: "PickRequestItems");

            migrationBuilder.DropTable(
                name: "ProductionHourlyDetails");

            migrationBuilder.DropTable(
                name: "RampAllocations");

            migrationBuilder.DropTable(
                name: "RampConfigurations");

            migrationBuilder.DropTable(
                name: "RampMetrics");

            migrationBuilder.DropTable(
                name: "SkuBatchUsages");

            migrationBuilder.DropTable(
                name: "SystemAlerts");

            migrationBuilder.DropTable(
                name: "SystemParameters");

            migrationBuilder.DropTable(
                name: "WavePickRequests");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Rejections");

            migrationBuilder.DropTable(
                name: "OperationalMetrics");

            migrationBuilder.DropTable(
                name: "ProductionHourlies");

            migrationBuilder.DropTable(
                name: "SkuBatches");

            migrationBuilder.DropTable(
                name: "Waves");

            migrationBuilder.DropTable(
                name: "Ramps");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PickRequests");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
