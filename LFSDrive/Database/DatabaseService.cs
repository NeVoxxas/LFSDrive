using LfsCruise.Core.Players;
using MySqlConnector;

namespace LfsCruise.Database;

public sealed class DatabaseService
{
    private readonly DatabaseConfig _config;

    public DatabaseService(DatabaseConfig config)
    {
        _config = config;
    }



    public async Task<(PlayerData Data, bool IsNew)> GetOrCreatePlayerAsync(Player player, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string selectSql = """
        SELECT id, money, bank, driven_distance, last_interest_at
        FROM players
        WHERE username = @username
        LIMIT 1;
        """;

        await using (var selectCommand = new MySqlCommand(selectSql, connection))
        {
            selectCommand.Parameters.AddWithValue("@username", player.Username);

            await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                var lastInterestOrdinal = reader.GetOrdinal("last_interest_at");

                var existingData = new PlayerData
                {
                    Id = reader.GetInt32("id"),
                    Money = reader.GetInt32("money"),
                    Bank = reader.GetInt32("bank"),
                    DrivenDistance = reader.GetDouble("driven_distance"),
                    LastInterestAt = reader.IsDBNull(lastInterestOrdinal)
                        ? null
                        : reader.GetDateTime(lastInterestOrdinal)
                };

                return (existingData, false);
            }
        }

        const string insertSql = """
        INSERT INTO players (username, nickname, money, bank, driven_distance)
        VALUES (@username, @nickname, 5000, 0, 0);
        """;

        await using (var insertCommand = new MySqlCommand(insertSql, connection))
        {
            insertCommand.Parameters.AddWithValue("@username", player.Username);
            insertCommand.Parameters.AddWithValue("@nickname", player.Nickname);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        var newData = new PlayerData
        {
            Money = 5000,
            Bank = 0,
            DrivenDistance = 0,
            LastInterestAt = null
        };

        return (newData, true);
    }

    public async Task<int> GetAdminLevelAsync(string username, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        SELECT level
        FROM admins
        WHERE username = @username
        LIMIT 1;
        """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", username);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result is null ? 0 : Convert.ToInt32(result);
    }

    public async Task SavePlayerAsync(Player player, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        UPDATE players
        SET nickname = @nickname,
            money = @money,
            bank = @bank,
            last_seen = CURRENT_TIMESTAMP,
            driven_distance = @driven_distance,
            last_interest_at = @last_interest_at
        WHERE username = @username;
        """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", player.Username);
        command.Parameters.AddWithValue("@nickname", player.Nickname);
        command.Parameters.AddWithValue("@money", player.Data.Money);
        command.Parameters.AddWithValue("@bank", player.Data.Bank);
        command.Parameters.AddWithValue("@driven_distance", player.Data.DrivenDistance);
        command.Parameters.AddWithValue("@last_interest_at", (object?)player.Data.LastInterestAt ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task AddBankBalanceAsync(int playerId, int amount, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
        UPDATE players
        SET bank = bank + @amount
        WHERE id = @id;
        """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@amount", amount);
        command.Parameters.AddWithValue("@id", playerId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}