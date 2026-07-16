namespace RewardStar.Core.Migrations.ProviderTypes;

public static class MigrationProviderTypes
{
    public const string Sqlite = "Microsoft.EntityFrameworkCore.Sqlite";
    public const string Postgres = "Npgsql.EntityFrameworkCore.PostgreSQL";

    private static readonly Dictionary<string, IMigrationProviderTypes> Strategies = new()
    {
        [Sqlite] = new SqliteMigrationProviderTypes(),
        [Postgres] = new PostgresMigrationProviderTypes()
    };

    public static IMigrationProviderTypes For(string activeProvider) =>
        Strategies.TryGetValue(activeProvider, out var strategy)
            ? strategy
            : throw new NotSupportedException($"No migration provider types registered for '{activeProvider}'.");
}
