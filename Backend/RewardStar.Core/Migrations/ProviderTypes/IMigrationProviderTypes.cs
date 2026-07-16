using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace RewardStar.Core.Migrations.ProviderTypes;

public interface IMigrationProviderTypes
{
    string Bool { get; }
    string DateTime { get; }
    string BoolLiteral(bool value);
    OperationBuilder<AddColumnOperation> AsAutoIncrementPrimaryKey(OperationBuilder<AddColumnOperation> column);

    /// <summary>
    /// SQL to run after inserting a row with an explicit PK value, so the provider's
    /// auto-increment sequence stays in sync. Null when the provider has nothing to reset (e.g. SQLite).
    /// </summary>
    string? ResetIdentitySequenceSql(string tableName, string idColumn);
}
