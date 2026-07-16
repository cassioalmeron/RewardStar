using System;
using Microsoft.EntityFrameworkCore.Migrations;
using RewardStar.Core.Migrations.ProviderTypes;

#nullable disable

namespace RewardStar.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var providerTypes = MigrationProviderTypes.For(migrationBuilder.ActiveProvider);
            var boolType = providerTypes.Bool;
            var dateTimeType = providerTypes.DateTime;

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = providerTypes.AsAutoIncrementPrimaryKey(table.Column<int>(nullable: false)),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Email = table.Column<string>(maxLength: 320, nullable: false),
                    Password = table.Column<string>(maxLength: 500, nullable: true),
                    GoogleAuthId = table.Column<string>(maxLength: 255, nullable: true),
                    Active = table.Column<bool>(type: boolType, nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: dateTimeType, nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: dateTimeType, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Activity",
                columns: table => new
                {
                    Id = providerTypes.AsAutoIncrementPrimaryKey(table.Column<int>(nullable: false)),
                    Description = table.Column<string>(maxLength: 500, nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Monday = table.Column<bool>(type: boolType, nullable: false),
                    Tuesday = table.Column<bool>(type: boolType, nullable: false),
                    Wednesday = table.Column<bool>(type: boolType, nullable: false),
                    Thursday = table.Column<bool>(type: boolType, nullable: false),
                    Friday = table.Column<bool>(type: boolType, nullable: false),
                    Position = table.Column<int>(nullable: false),
                    Active = table.Column<bool>(type: boolType, nullable: false, defaultValue: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activity_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activity_UserId",
                table: "Activity",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_UserId_Position",
                table: "Activity",
                columns: new[] { "UserId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_GoogleAuthId",
                table: "User",
                column: "GoogleAuthId",
                unique: true,
                filter: "\"GoogleAuthId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activity");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
