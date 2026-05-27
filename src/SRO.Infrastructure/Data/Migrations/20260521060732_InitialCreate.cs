using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRO.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SportlinkClientId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClubRelatieCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamCode = table.Column<int>(type: "int", nullable: false),
                    TeamNaam = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LeeftijdsCategorie = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Klasse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpelSoort = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSupplier = table.Column<bool>(type: "bit", nullable: false),
                    SupplierCategorie = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WedstrijdCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WedstrijdDatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AanvangsTijd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ThuisTeam = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UitTeam = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Klasse = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Competitie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Accommodatie = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Veld = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Plaats = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Scheidsrechters = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsThuis = table.Column<bool>(type: "bit", nullable: false),
                    AssignedTeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_AssignedTeamId",
                        column: x => x.AssignedTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Matches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsManualOverride = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assignments_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_MatchId",
                table: "Assignments",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TeamId",
                table: "Assignments",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TenantId_MatchId",
                table: "Assignments",
                columns: new[] { "TenantId", "MatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_AssignedTeamId",
                table: "Matches",
                column: "AssignedTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TenantId_WedstrijdCode",
                table: "Matches",
                columns: new[] { "TenantId", "WedstrijdCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TenantId_TeamCode",
                table: "Teams",
                columns: new[] { "TenantId", "TeamCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
