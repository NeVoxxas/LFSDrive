using LfsCruise.Core.Players;
using LfsCruise.Database;
using MySqlConnector;

namespace LfsCruise.Core.Vehicles;

public sealed class VehicleOwnershipService
{
    private readonly DatabaseConfig _config;

    public VehicleOwnershipService(DatabaseConfig config)
    {
        _config = config;
    }

    public async Task<bool> OwnsVehicleAsync(Player player, string carName, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"CHECK OWNERSHIP: playerId={player.Data.Id}, car={carName}");

        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT COUNT(*)
            FROM owned_vehicles
            WHERE player_id = @player_id
              AND car_code = @car_code;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@player_id", player.Data.Id);
        command.Parameters.AddWithValue("@car_code", carName);

        var result = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

        return result > 0;
    }
    public async Task AddVehicleAsync(Player player, string carName, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
           INSERT IGNORE INTO owned_vehicles (player_id, car_code)
           VALUES (@player_id, @car_code);
           """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@player_id", player.Data.Id);
        command.Parameters.AddWithValue("@car_code", carName);

        await command.ExecuteScalarAsync(cancellationToken);
    }

    public async Task TransferVehicleAsync(
        int sellerPlayerId, int buyerPlayerId, string carCode, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            UPDATE owned_vehicles
            SET player_id = @buyer_id
            WHERE player_id = @seller_id
              AND car_code = @car_code;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@buyer_id", buyerPlayerId);
        command.Parameters.AddWithValue("@seller_id", sellerPlayerId);
        command.Parameters.AddWithValue("@car_code", carCode);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<string>> GetOwnedVehiclesAsync(Player player, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        SELECT car_code
        FROM owned_vehicles
        WHERE player_id = @player_id
        ORDER BY car_code;
        """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@player_id", player.Data.Id);

        var result = new List<string>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    public async Task<bool> RemoveVehicleAsync(Player player, string carCode, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        DELETE FROM owned_vehicles
        WHERE player_id = @player_id
          AND car_code = @car_code;
        """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@player_id", player.Data.Id);
        command.Parameters.AddWithValue("@car_code", carCode);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected > 0;
    }
}