using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WCS_PG.Data.Migrations
{
    /// <inheritdoc />
    public partial class Rafael_007 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkReference",
                table: "PickRequestItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkReference",
                table: "PickRequestItems");
        }
    }
}
