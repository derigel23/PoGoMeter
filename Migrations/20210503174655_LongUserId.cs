using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PoGoMeter.Migrations
{
    public partial class LongUserId : Migration
    {
        protected void Before(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey("PK_Ignore", "Ignore", TargetModel.GetDefaultSchema());
        }

        protected void After(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey("PK_Ignore", "Ignore", new [] { "UserId", "Pokemon" }, TargetModel.GetDefaultSchema());
        }
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            Before(migrationBuilder);
            
            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                schema: "PoGoMeter",
                table: "Ignore",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
            
            After(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            Before(migrationBuilder);
            
            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                schema: "PoGoMeter",
                table: "Ignore",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
            
            After(migrationBuilder);
        }
    }
}
