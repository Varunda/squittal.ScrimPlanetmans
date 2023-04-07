using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace squittal.ScrimPlanetmans.App.Migrations
{
    public partial class AddRevives : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
        public string ScrimMatchId { get; set; }
        public int ScrimMatchRound { get; set; }
        public int TeamOrdinal { get; set; }
        public int FacilityCapturePoints { get; set; }

        public int GrenadeAssists { get; set; }
        public int SpotAssists { get; set; }

        public int GrenadeTeamAssists { get; set; }
        
        */
            migrationBuilder.CreateTable(
                name: "ScrimMatchReportInfantryTeamRoundStats",
                columns: table => new
                {
                    ScrimMatchId = table.Column<string>(nullable: false),
                    ScrimMatchRound = table.Column<string>(nullable: false),
                    TeamOrdinal = table.Column<string>(nullable: false),
                    FacilityCapturePoints = table.Column<string>(nullable: false),
                    GrenadeAssists = table.Column<int>(nullable: false),
                    SpotAssists = table.Column<int>(nullable: false),
                    GrenadeTeamAssists = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    //table.PrimaryKey("PK_Faction", x => x.Id);
                });
            
            /*
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
                defaultValue: 0);*/

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(name: "Revived", table: "ScrimMatchReportStats");
            //migrationBuilder.DropColumn(name: "Revives", table: "ScrimMatchReportStats");
        }

    }
}
