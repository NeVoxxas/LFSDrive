using LfsCruise.Database;
using MySqlConnector;

namespace LfsCruise.Core.Economy.Bank;

public sealed class BankTransactionStorage
{
    private readonly DatabaseConfig _config;

    public BankTransactionStorage(DatabaseConfig config)
    {
        _config = config;
    }

    public async Task AddAsync(BankTransaction transaction, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            INSERT INTO bank_transactions (player_id, type, amount, balance_after, created_at)
            VALUES (@player_id, @type, @amount, @balance_after, @created_at);
            """;

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@player_id", transaction.PlayerId);
        command.Parameters.AddWithValue("@type", (int)transaction.Type);
        command.Parameters.AddWithValue("@amount", transaction.Amount);
        command.Parameters.AddWithValue("@balance_after", transaction.BalanceAfter);
        command.Parameters.AddWithValue("@created_at", transaction.CreatedAt);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<BankTransaction> Items, int TotalCount)> GetPageAsync(
        int playerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(_config.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string countSql = "SELECT COUNT(*) FROM bank_transactions WHERE player_id = @player_id;";

        int totalCount;

        await using (var countCommand = new MySqlCommand(countSql, connection))
        {
            countCommand.Parameters.AddWithValue("@player_id", playerId);
            totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));
        }

        const string selectSql = """
            SELECT id, player_id, type, amount, balance_after, created_at
            FROM bank_transactions
            WHERE player_id = @player_id
            ORDER BY created_at DESC, id DESC
            LIMIT @limit OFFSET @offset;
            """;

        var items = new List<BankTransaction>();

        await using (var selectCommand = new MySqlCommand(selectSql, connection))
        {
            selectCommand.Parameters.AddWithValue("@player_id", playerId);
            selectCommand.Parameters.AddWithValue("@limit", pageSize);
            selectCommand.Parameters.AddWithValue("@offset", Math.Max(0, page) * pageSize);

            await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                items.Add(new BankTransaction
                {
                    Id = reader.GetInt32("id"),
                    PlayerId = reader.GetInt32("player_id"),
                    Type = (BankTransactionType)reader.GetInt32("type"),
                    Amount = reader.GetInt32("amount"),
                    BalanceAfter = reader.GetInt32("balance_after"),
                    CreatedAt = reader.GetDateTime("created_at")
                });
            }
        }

        return (items, totalCount);
    }
}