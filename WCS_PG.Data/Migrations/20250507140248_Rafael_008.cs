using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WCS_PG.Data.Migrations
{
    /// <inheritdoc />
    public partial class Rafael_008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarMoveId",
                table: "PickRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarMoveId",
                table: "PickRequests");
        }
    }
}
