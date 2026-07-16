using Microsoft.EntityFrameworkCore.Migrations;
using RewardStar.Core.Constants;
using RewardStar.Core.Migrations.ProviderTypes;

#nullable disable

namespace RewardStar.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var providerTypes = MigrationProviderTypes.For(migrationBuilder.ActiveProvider);

            // Hash the admin password using BCrypt with cost factor 12
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin@123", 12);

            // Insert admin user with explicit ID = ADMIN_USER_ID
            var currentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            var activeValue = providerTypes.BoolLiteral(true);

            migrationBuilder.Sql($@"
                INSERT INTO ""User"" (""Id"", ""Name"", ""Email"", ""Password"", ""GoogleAuthId"", ""Active"", ""CreatedAt"", ""LastLoginAt"")
                VALUES ({UserConstants.ADMIN_USER_ID}, 'Administrator', 'admin@rewardstar.com', '{hashedPassword}', NULL, {activeValue}, '{currentDateTime}', NULL);
            ");

            // Ensure the identity sequence starts after the explicit Id we just inserted;
            // no-op on providers without sequences (e.g. SQLite).
            var resetSequenceSql = providerTypes.ResetIdentitySequenceSql("User", "Id");
            if (!string.IsNullOrEmpty(resetSequenceSql))
                migrationBuilder.Sql(resetSequenceSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove admin user
            migrationBuilder.Sql($@"DELETE FROM ""User"" WHERE ""Id"" = {UserConstants.ADMIN_USER_ID};");
        }
    }
}
