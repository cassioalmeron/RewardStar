using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace RewardStar.Core.Migrations.ProviderTypes;

public class PostgresMigrationProviderTypes : IMigrationProviderTypes
{
    public string Bool => "boolean";
    public string DateTime => "timestamp without time zone";

    public string BoolLiteral(bool value) => value ? "true" : "false";

    public OperationBuilder<AddColumnOperation> AsAutoIncrementPrimaryKey(OperationBuilder<AddColumnOperation> column) =>
        column.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

    public string? ResetIdentitySequenceSql(string tableName, string idColumn) => $@"
        SELECT setval(pg_get_serial_sequence('""{tableName}""', '{idColumn}'), COALESCE(MAX(""{idColumn}""), 1)) FROM ""{tableName}"";
    ";
}
