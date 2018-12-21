using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PoGoMeter.Migrations
{
    public partial class BaseStatsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attack",
                schema: "PoGoMeter",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Defense",
                schema: "PoGoMeter",
                table: "Stats");

            migrationBuilder.DropColumn(
                name: "Stamina",
                schema: "PoGoMeter",
                table: "Stats");

            migrationBuilder.CreateTable(
                name: "BaseStats",
                schema: "PoGoMeter",
                columns: table => new
                {
                    Pokemon = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Attack = table.Column<short>(nullable: false),
                    Defense = table.Column<short>(nullable: false),
                    Stamina = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseStats", x => x.Pokemon);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Stats_BaseStats_Pokemon",
                schema: "PoGoMeter",
                table: "Stats",
                column: "Pokemon",
                principalSchema: "PoGoMeter",
                principalTable: "BaseStats",
                principalColumn: "Pokemon",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stats_BaseStats_Pokemon",
                schema: "PoGoMeter",
                table: "Stats");

            migrationBuilder.DropTable(
                name: "BaseStats",
                schema: "PoGoMeter");

            migrationBuilder.AddColumn<short>(
                name: "Attack",
                schema: "PoGoMeter",
                table: "Stats",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Defense",
                schema: "PoGoMeter",
                table: "Stats",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "Stamina",
                schema: "PoGoMeter",
                table: "Stats",
                nullable: false,
                defaultValue: (short)0);
        }
    }
}
