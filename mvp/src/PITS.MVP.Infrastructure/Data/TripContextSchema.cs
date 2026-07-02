using Microsoft.EntityFrameworkCore;

namespace PITS.MVP.Infrastructure.Data;

public static class TripContextSchema
{
    public static void EnsureReady(TripContext context)
    {
        context.Database.EnsureCreated();

        Execute(context, """
CREATE TABLE IF NOT EXISTS TripPlans (
    Id TEXT NOT NULL CONSTRAINT PK_TripPlans PRIMARY KEY,
    Title TEXT NOT NULL,
    StartsAt TEXT NOT NULL,
    EndsAt TEXT NULL,
    Timezone TEXT NOT NULL,
    LocationName TEXT NULL,
    Location BLOB NULL,
    GeoHash TEXT NULL,
    Notes TEXT NULL,
    ActivityType TEXT NOT NULL,
    Visibility TEXT NOT NULL,
    Source TEXT NOT NULL,
    ReminderAt TEXT NULL,
    Status TEXT NOT NULL,
    ExternalId TEXT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL
);
""");

        Execute(context, """
CREATE TABLE IF NOT EXISTS ImportStagingItems (
    Id TEXT NOT NULL CONSTRAINT PK_ImportStagingItems PRIMARY KEY,
    Source TEXT NOT NULL,
    Fingerprint TEXT NOT NULL,
    ExternalId TEXT NULL,
    StartsAt TEXT NOT NULL,
    EndsAt TEXT NULL,
    Title TEXT NOT NULL,
    LocationName TEXT NULL,
    Location BLOB NULL,
    RawPayload TEXT NULL,
    Status TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    ConfirmedTripId TEXT NULL,
    ConfirmedPlanId TEXT NULL
);
""");

        AddColumn(context, "Trips", "PlaceId", "TEXT NULL");
        AddColumn(context, "Trips", "PlanId", "TEXT NULL");
        AddColumn(context, "Trips", "Source", "TEXT NOT NULL DEFAULT 'Manual'");
        AddColumn(context, "Places", "Radius", "REAL NULL DEFAULT 200");

        Execute(context, "CREATE INDEX IF NOT EXISTS IX_TripPlans_StartsAt ON TripPlans (StartsAt);");
        Execute(context, "CREATE INDEX IF NOT EXISTS IX_TripPlans_Status ON TripPlans (Status);");
        Execute(context, "CREATE INDEX IF NOT EXISTS IX_TripPlans_Source ON TripPlans (Source);");
        Execute(context, "CREATE INDEX IF NOT EXISTS IX_TripPlans_ExternalId ON TripPlans (ExternalId);");
        Execute(context, "CREATE UNIQUE INDEX IF NOT EXISTS IX_ImportStagingItems_Fingerprint ON ImportStagingItems (Fingerprint);");
        Execute(context, "CREATE INDEX IF NOT EXISTS IX_ImportStagingItems_Status ON ImportStagingItems (Status);");
        Execute(context, "CREATE INDEX IF NOT EXISTS IX_ImportStagingItems_StartsAt ON ImportStagingItems (StartsAt);");
        Execute(context, "CREATE INDEX IF NOT EXISTS IX_Trips_PlaceId ON Trips (PlaceId);");
        Execute(context, "CREATE INDEX IF NOT EXISTS IX_Trips_PlanId ON Trips (PlanId);");
    }

    private static void AddColumn(TripContext context, string table, string column, string definition)
    {
        if (!TableExists(context, table) || ColumnExists(context, table, column)) return;
        Execute(context, $"ALTER TABLE {table} ADD COLUMN {column} {definition};");
    }

    private static bool TableExists(TripContext context, string table)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=$table";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$table";
        parameter.Value = table;
        command.Parameters.Add(parameter);
        Open(context);
        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static bool ColumnExists(TripContext context, string table, string column)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        Open(context);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static void Execute(TripContext context, string sql)
    {
        Open(context);
        context.Database.ExecuteSqlRaw(sql);
    }

    private static void Open(TripContext context)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();
    }
}
