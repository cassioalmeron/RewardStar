using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardStart.Core.Migrations
{
    /// <inheritdoc />
    public partial class ActivityAddPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "Activity",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "Activity");
        }
    }
}
