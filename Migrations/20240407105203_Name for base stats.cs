using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoGoMeter.Migrations
{
    public partial class Nameforbasestats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_PokemonName_BaseStats_Pokemon",
                schema: "PoGoMeter",
                table: "PokemonName",
                column: "Pokemon",
                principalSchema: "PoGoMeter",
                principalTable: "BaseStats",
                principalColumn: "Pokemon",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PokemonName_BaseStats_Pokemon",
                schema: "PoGoMeter",
                table: "PokemonName");
        }
    }
}
