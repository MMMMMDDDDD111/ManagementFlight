using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightManagement.Migrations
{
    public partial class VI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UpdateVersionId",
                table: "DocumentInfo",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UpdateVersion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocID = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentInformationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UpdateVersion_DocumentInfo_DocumentInformationId",
                        column: x => x.DocumentInformationId,
                        principalTable: "DocumentInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UpdateVersion_DocumentInformationId",
                table: "UpdateVersion",
                column: "DocumentInformationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpdateVersion");

            migrationBuilder.DropColumn(
                name: "UpdateVersionId",
                table: "DocumentInfo");
        }
    }
}
