using Microsoft.EntityFrameworkCore.Migrations;

namespace PoGoMeter.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "PoGoMeter");

            migrationBuilder.CreateTable(
                name: "Stats",
                schema: "PoGoMeter",
                columns: table => new
                {
                    Pokemon = table.Column<short>(nullable: false),
                    AttackIV = table.Column<byte>(nullable: false),
                    DefenseIV = table.Column<byte>(nullable: false),
                    StaminaIV = table.Column<byte>(nullable: false),
                    Level = table.Column<byte>(nullable: false),
                    CP = table.Column<short>(nullable: false),
                    Attack = table.Column<short>(nullable: false),
                    Defense = table.Column<short>(nullable: false),
                    Stamina = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats", x => new { x.Pokemon, x.CP, x.Level, x.AttackIV, x.DefenseIV, x.StaminaIV });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stats",
                schema: "PoGoMeter");
        }
    }
}
