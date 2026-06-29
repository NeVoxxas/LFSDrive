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

    public async Task<PlayerData> GetOrCreatePlayerAsync(Player player, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string selectSql = """
            SELECT money, bank
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
                return new PlayerData
                {
                    Money = reader.GetInt32("money"),
                    Bank = reader.GetInt32("bank")
                };
            }
        }

        const string insertSql = """
            INSERT INTO players (username, nickname, money, bank)
            VALUES (@username, @nickname, 5000, 0);
            """;

        await using (var insertCommand = new MySqlCommand(insertSql, connection))
        {
            insertCommand.Parameters.AddWithValue("@username", player.Username);
            insertCommand.Parameters.AddWithValue("@nickname", player.Nickname);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        return new PlayerData
        {
            Money = 5000,
            Bank = 0
        };
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
            last_seen = CURRENT_TIMESTAMP
        WHERE username = @username;
        """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@username", player.Username);
        command.Parameters.AddWithValue("@nickname", player.Nickname);
        command.Parameters.AddWithValue("@money", player.Data.Money);
        command.Parameters.AddWithValue("@bank", player.Data.Bank);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

}