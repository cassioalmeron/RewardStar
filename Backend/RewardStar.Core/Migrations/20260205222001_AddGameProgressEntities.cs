using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RewardStar.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddGameProgressEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cross-database compatibility: type detection
            var isSqlite = migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite";
            var boolType = isSqlite ? "INTEGER" : "boolean";
            var dateTimeType = isSqlite ? "TEXT" : "timestamp without time zone";

            migrationBuilder.DropIndex(
                name: "IX_User_GoogleAuthId",
                table: "User");

            migrationBuilder.CreateTable(
                name: "GameCompletion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Day = table.Column<int>(type: "INTEGER", nullable: false),
                    Completed = table.Column<bool>(type: boolType, nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameCompletion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameCompletion_Activity_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameCompletion_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameState",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPoints = table.Column<int>(type: "INTEGER", nullable: false),
                    Balance = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameState", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameState_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RewardClaim",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RewardType = table.Column<int>(type: "INTEGER", nullable: false),
                    PointsCost = table.Column<int>(type: "INTEGER", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: dateTimeType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardClaim", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardClaim_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_GoogleAuthId",
                table: "User",
                column: "GoogleAuthId",
                unique: true,
                filter: "\"GoogleAuthId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GameCompletion_ActivityId_Day_UserId",
                table: "GameCompletion",
                columns: new[] { "ActivityId", "Day", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameCompletion_UserId",
                table: "GameCompletion",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameState_UserId",
                table: "GameState",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RewardClaim_UserId",
                table: "RewardClaim",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameCompletion");

            migrationBuilder.DropTable(
                name: "GameState");

            migrationBuilder.DropTable(
                name: "RewardClaim");

            migrationBuilder.DropIndex(
                name: "IX_User_GoogleAuthId",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_GoogleAuthId",
                table: "User",
                column: "GoogleAuthId",
                unique: true,
                filter: "GoogleAuthId IS NOT NULL");
        }
    }
}
