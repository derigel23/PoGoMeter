using Microsoft.EntityFrameworkCore.Migrations;

namespace PoGoMeter.Migrations
{
    public partial class PokemonNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PokemonName",
                schema: "PoGoMeter",
                columns: table => new
                {
                    Pokemon = table.Column<short>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PokemonName", x => x.Pokemon);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PokemonName",
                schema: "PoGoMeter");
        }
    }
}
