namespace LfsCruise.Database;

public sealed class DatabaseConfig
{
    public string Host { get; init; } = "127.0.0.1";
    public int Port { get; init; } = 3306;
    public string Database { get; init; } = "lfsdrive";
    public string Username { get; init; } = "root";
    public string Password { get; init; } = "";

    public string ConnectionString =>
        $"Server={Host};Port={Port};Database={Database};User ID={Username};Password={Password};Pooling=true;";
}