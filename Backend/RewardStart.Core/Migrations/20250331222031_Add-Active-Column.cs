using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RewardStart.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Activity",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "Activity");
        }
    }
}
