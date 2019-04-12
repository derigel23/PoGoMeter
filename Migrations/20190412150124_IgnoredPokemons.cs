using Microsoft.EntityFrameworkCore.Migrations;

namespace PoGoMeter.Migrations
{
    public partial class IgnoredPokemons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ignore",
                schema: "PoGoMeter",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    Pokemon = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ignore", x => new { x.UserId, x.Pokemon });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ignore_UserId",
                schema: "PoGoMeter",
                table: "Ignore",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ignore",
                schema: "PoGoMeter");
        }
    }
}
