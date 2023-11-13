using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightManagement.Migrations
{
    public partial class VI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentInfo_Group_GroupsGroupId",
                table: "DocumentInfo");

            migrationBuilder.DropIndex(
                name: "IX_DocumentInfo_GroupsGroupId",
                table: "DocumentInfo");

            migrationBuilder.DropColumn(
                name: "GroupsGroupId",
                table: "DocumentInfo");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "DocumentInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentInfo_GroupId",
                table: "DocumentInfo",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentInfo_Group_GroupId",
                table: "DocumentInfo",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "GroupId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentInfo_Group_GroupId",
                table: "DocumentInfo");

            migrationBuilder.DropIndex(
                name: "IX_DocumentInfo_GroupId",
                table: "DocumentInfo");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "DocumentInfo");

            migrationBuilder.AddColumn<int>(
                name: "GroupsGroupId",
                table: "DocumentInfo",
                type: "int",
                nullable: true);

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
    }
}
