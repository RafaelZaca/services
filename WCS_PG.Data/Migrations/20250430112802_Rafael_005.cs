using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WCS_PG.Data.Migrations
{
    /// <inheritdoc />
    public partial class Rafael_005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssetType",
                table: "PickRequestItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PackageUnit",
                table: "PickRequestItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourcePallet",
                table: "PickRequestItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetType",
                table: "PickRequestItems");

            migrationBuilder.DropColumn(
                name: "PackageUnit",
                table: "PickRequestItems");

            migrationBuilder.DropColumn(
                name: "SourcePallet",
                table: "PickRequestItems");
        }
    }
}
