using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WCS_PG.Data.Migrations
{
    /// <inheritdoc />
    public partial class Rafael_002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ramps_PickRequests_CurrentPickRequestId",
                table: "Ramps");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentPickRequestId",
                table: "Ramps",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Ramps_PickRequests_CurrentPickRequestId",
                table: "Ramps",
                column: "CurrentPickRequestId",
                principalTable: "PickRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ramps_PickRequests_CurrentPickRequestId",
                table: "Ramps");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentPickRequestId",
                table: "Ramps",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Ramps_PickRequests_CurrentPickRequestId",
                table: "Ramps",
                column: "CurrentPickRequestId",
                principalTable: "PickRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
