using LfsCruise.Core.Players;
using LfsCruise.Database;
using MySqlConnector;

namespace LfsCruise.Core.Vehicles.Regitra;

public sealed class RegitraStorage
{
    private readonly DatabaseConfig _config;

    public RegitraStorage(DatabaseConfig config)
    {
        _config = config;
    }

    public async Task<VehicleDocuments?> GetDocumentsAsync(
        Player player, string carCode, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT plate_number, insurance_expires_at, inspection_expires_at
            FROM owned_vehicles
            WHERE player_id = @player_id
              AND car_code = @car_code
            LIMIT 1;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@player_id", player.Data.Id);
        command.Parameters.AddWithValue("@car_code", carCode);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var plateOrdinal = reader.GetOrdinal("plate_number");
        var insuranceOrdinal = reader.GetOrdinal("insurance_expires_at");
        var inspectionOrdinal = reader.GetOrdinal("inspection_expires_at");

        return new VehicleDocuments
        {
            PlateNumber = reader.IsDBNull(plateOrdinal) ? null : reader.GetString(plateOrdinal),
            InsuranceExpiresAt = reader.IsDBNull(insuranceOrdinal) ? null : reader.GetDateTime(insuranceOrdinal),
            InspectionExpiresAt = reader.IsDBNull(inspectionOrdinal) ? null : reader.GetDateTime(inspectionOrdinal)
        };
    }

    public async Task<bool> IsPlateTakenAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT COUNT(*)
            FROM owned_vehicles
            WHERE plate_number = @plate_number;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@plate_number", plateNumber);

        var result = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

        return result > 0;
    }

    public async Task SetPlateAsync(
        Player player, string carCode, string plateNumber, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE owned_vehicles
            SET plate_number = @plate_number
            WHERE player_id = @player_id
              AND car_code = @car_code;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@plate_number", plateNumber);
        command.Parameters.AddWithValue("@player_id", player.Data.Id);
        command.Parameters.AddWithValue("@car_code", carCode);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetInsuranceAsync(
        Player player, string carCode, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE owned_vehicles
            SET insurance_expires_at = @expires_at
            WHERE player_id = @player_id
              AND car_code = @car_code;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@expires_at", expiresAt);
        command.Parameters.AddWithValue("@player_id", player.Data.Id);
        command.Parameters.AddWithValue("@car_code", carCode);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetInspectionAsync(
        Player player, string carCode, DateTime expiresAt, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE owned_vehicles
            SET inspection_expires_at = @expires_at
            WHERE player_id = @player_id
              AND car_code = @car_code;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@expires_at", expiresAt);
        command.Parameters.AddWithValue("@player_id", player.Data.Id);
        command.Parameters.AddWithValue("@car_code", carCode);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}