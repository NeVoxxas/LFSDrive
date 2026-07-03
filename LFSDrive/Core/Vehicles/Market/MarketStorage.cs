using LfsCruise.Database;
using MySqlConnector;

namespace LfsCruise.Core.Vehicles.Market;

// Reikalinga lentelė (sukurti rankiniu būdu, kaip ir kitos lentelės šiame projekte):
//
// CREATE TABLE market_listings (
//   id INT AUTO_INCREMENT PRIMARY KEY,
//   seller_player_id INT NOT NULL,
//   seller_username VARCHAR(50) NOT NULL,
//   car_code VARCHAR(20) NOT NULL,
//   display_name VARCHAR(100) NOT NULL,
//   category_id VARCHAR(10) NOT NULL,
//   price INT NOT NULL,
//   created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
// );

public sealed class MarketStorage
{
    private readonly DatabaseConfig _config;

    public MarketStorage(DatabaseConfig config)
    {
        _config = config;
    }

    public async Task AddAsync(MarketListing listing, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO market_listings
                (seller_player_id, seller_username, car_code, display_name, category_id, price, created_at)
            VALUES
                (@seller_player_id, @seller_username, @car_code, @display_name, @category_id, @price, @created_at);
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@seller_player_id", listing.SellerPlayerId);
        command.Parameters.AddWithValue("@seller_username", listing.SellerUsername);
        command.Parameters.AddWithValue("@car_code", listing.CarCode);
        command.Parameters.AddWithValue("@display_name", listing.DisplayName);
        command.Parameters.AddWithValue("@category_id", listing.CategoryId);
        command.Parameters.AddWithValue("@price", listing.Price);
        command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> IsCarListedAsync(int sellerPlayerId, string carCode, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT COUNT(*)
            FROM market_listings
            WHERE seller_player_id = @seller_player_id
              AND car_code = @car_code;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@seller_player_id", sellerPlayerId);
        command.Parameters.AddWithValue("@car_code", carCode);

        var result = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

        return result > 0;
    }

    public async Task<MarketListing?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT id, seller_player_id, seller_username, car_code, display_name, category_id, price, created_at
            FROM market_listings
            WHERE id = @id
            LIMIT 1;
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return Read(reader);
    }

    public async Task<bool> RemoveAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "DELETE FROM market_listings WHERE id = @id;";

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected > 0;
    }

    public async Task<(IReadOnlyList<MarketListing> Items, int TotalCount)> GetPageAsync(
        string categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string countSql = "SELECT COUNT(*) FROM market_listings WHERE category_id = @category_id;";

        int totalCount;

        await using (var countCommand = new MySqlCommand(countSql, connection))
        {
            countCommand.Parameters.AddWithValue("@category_id", categoryId);
            totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));
        }

        const string selectSql = """
            SELECT id, seller_player_id, seller_username, car_code, display_name, category_id, price, created_at
            FROM market_listings
            WHERE category_id = @category_id
            ORDER BY created_at ASC, id ASC
            LIMIT @limit OFFSET @offset;
            """;

        var items = new List<MarketListing>();

        await using (var selectCommand = new MySqlCommand(selectSql, connection))
        {
            selectCommand.Parameters.AddWithValue("@category_id", categoryId);
            selectCommand.Parameters.AddWithValue("@limit", pageSize);
            selectCommand.Parameters.AddWithValue("@offset", Math.Max(0, page) * pageSize);

            await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(Read(reader));
            }
        }

        return (items, totalCount);
    }

    private static MarketListing Read(MySqlDataReader reader)
    {
        return new MarketListing
        {
            Id = reader.GetInt32("id"),
            SellerPlayerId = reader.GetInt32("seller_player_id"),
            SellerUsername = reader.GetString("seller_username"),
            CarCode = reader.GetString("car_code"),
            DisplayName = reader.GetString("display_name"),
            CategoryId = reader.GetString("category_id"),
            Price = reader.GetInt32("price"),
            CreatedAt = reader.GetDateTime("created_at")
        };
    }
}