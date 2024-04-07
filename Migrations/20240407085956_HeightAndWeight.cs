using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoGoMeter.Migrations
{
    public partial class HeightAndWeight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                schema: "PoGoMeter",
                table: "BaseStats",
                type: "decimal(9,3)",
                nullable: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                schema: "PoGoMeter",
                table: "BaseStats",
                type: "decimal(9,3)",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                schema: "PoGoMeter",
                table: "BaseStats");

            migrationBuilder.DropColumn(
                name: "Height",
                schema: "PoGoMeter",
                table: "BaseStats");
        }
    }
}
