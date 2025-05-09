using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WCS_PG.Data.Migrations
{
    /// <inheritdoc />
    public partial class Rafael_004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupPermission");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermission_PermissionsId",
                table: "GroupPermission",
                column: "PermissionsId");
        }
    }
}
