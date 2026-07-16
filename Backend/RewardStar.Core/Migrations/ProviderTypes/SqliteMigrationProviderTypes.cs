using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace RewardStar.Core.Migrations.ProviderTypes;

public class SqliteMigrationProviderTypes : IMigrationProviderTypes
{
    public string Bool => "INTEGER";
    public string DateTime => "TEXT";

    public string BoolLiteral(bool value) => value ? "1" : "0";

    public OperationBuilder<AddColumnOperation> AsAutoIncrementPrimaryKey(OperationBuilder<AddColumnOperation> column) =>
        column.Annotation("Sqlite:Autoincrement", true);

    public string? ResetIdentitySequenceSql(string tableName, string idColumn) => null;
}
