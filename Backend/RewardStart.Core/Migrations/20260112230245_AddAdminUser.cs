using Microsoft.EntityFrameworkCore.Migrations;
using RewardStart.Core.Constants;

#nullable disable

namespace RewardStart.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Detect database provider for cross-database compatibility
            var isSqlite = migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite";
            var isPostgres = migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL";

            // Hash the admin password using BCrypt with cost factor 12
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin@123", 12);

            // Insert admin user with explicit ID = ADMIN_USER_ID
            var currentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            // Use provider-specific boolean value for Active column
            var activeValue = isSqlite ? "1" : "true";

            migrationBuilder.Sql($@"
                INSERT INTO ""User"" (""Id"", ""Name"", ""Email"", ""Password"", ""GoogleAuthId"", ""Active"", ""CreatedAt"", ""LastLoginAt"")
                VALUES ({UserConstants.ADMIN_USER_ID}, 'Administrator', 'admin@rewardstar.com', '{hashedPassword}', NULL, {activeValue}, '{currentDateTime}', NULL);
            ");

            // For PostgreSQL: Ensure the sequence starts from 2 to avoid conflicts
            // This will be ignored by SQLite (which doesn't have sequences)
            if (isPostgres)
            {
                migrationBuilder.Sql(@"
                    SELECT setval(pg_get_serial_sequence('""User""', 'Id'), COALESCE(MAX(""Id""), 1)) FROM ""User"";
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove admin user
            migrationBuilder.Sql($@"DELETE FROM ""User"" WHERE ""Id"" = {UserConstants.ADMIN_USER_ID};");
        }
    }
}
