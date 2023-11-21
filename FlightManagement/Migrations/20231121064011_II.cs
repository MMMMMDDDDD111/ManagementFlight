using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightManagement.Migrations
{
    public partial class II : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Login_Group_GroupsGroupId",
                table: "Login");

            migrationBuilder.DropIndex(
                name: "IX_Login_GroupsGroupId",
                table: "Login");

            migrationBuilder.DropColumn(
                name: "GroupsGroupId",
                table: "Login");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Login",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupsGroupId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_GroupsGroupId",
                table: "AspNetUsers",
                column: "GroupsGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Group_GroupsGroupId",
                table: "AspNetUsers",
                column: "GroupsGroupId",
                principalTable: "Group",
                principalColumn: "GroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Group_GroupsGroupId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_GroupsGroupId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GroupsGroupId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Login",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "GroupsGroupId",
                table: "Login",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Login_GroupsGroupId",
                table: "Login",
                column: "GroupsGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Login_Group_GroupsGroupId",
                table: "Login",
                column: "GroupsGroupId",
                principalTable: "Group",
                principalColumn: "GroupId");
        }
    }
}
