using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace squittal.ScrimPlanetmans.App.Migrations
{
    public partial class AddRevives : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Revived",
                table: "ScrimMatchReportInfantryTeamRoundStats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Revives",
                table: "ScrimMatchReportInfantryTeamRoundStats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Revived",
                table: "ScrimMatchReportInfantryPlayerRoundStats",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Revives",
                table: "ScrimMatchReportInfantryPlayerRoundStats",
                nullable: false,
                defaultValue: 0);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Revived", table: "ScrimMatchReportStats");
            migrationBuilder.DropColumn(name: "Revives", table: "ScrimMatchReportStats");
        }

    }
}
