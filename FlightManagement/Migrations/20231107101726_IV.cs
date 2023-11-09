using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightManagement.Migrations
{
    public partial class IV : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Permission",
                table: "DocumentInfo",
                newName: "GroupsGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentInfo_GroupsGroupId",
                table: "DocumentInfo",
                column: "GroupsGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentInfo_Group_GroupsGroupId",
                table: "DocumentInfo",
                column: "GroupsGroupId",
                principalTable: "Group",
                principalColumn: "GroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentInfo_Group_GroupsGroupId",
                table: "DocumentInfo");

            migrationBuilder.DropIndex(
                name: "IX_DocumentInfo_GroupsGroupId",
                table: "DocumentInfo");

            migrationBuilder.RenameColumn(
                name: "GroupsGroupId",
                table: "DocumentInfo",
                newName: "Permission");
        }
    }
}
