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
}