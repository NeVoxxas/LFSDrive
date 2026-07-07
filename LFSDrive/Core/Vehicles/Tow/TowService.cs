using LfsCruise.Core.Economy;
using LfsCruise.Core.Players;
using LfsCruise.Database;

namespace LfsCruise.Core.Vehicles.Tow;
public sealed class TowService
{
    public const double MaxStopSpeedKmh = 1.0;

    private readonly TowConfigStorage _configStorage;
    private readonly EconomyService _economyService;
    private readonly DatabaseService _databaseService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;
    private readonly Func<byte, CancellationToken, Task> _sendCarReset;

    private TowConfig _config;

    public TowService(
        TowConfigStorage configStorage,
        EconomyService economyService,
        DatabaseService databaseService,
        Func<byte, string, CancellationToken, Task> sendMessage,
        Func<byte, CancellationToken, Task> sendCarReset)
    {
        _configStorage = configStorage;
        _economyService = economyService;
        _databaseService = databaseService;
        _sendMessage = sendMessage;
        _sendCarReset = sendCarReset;

        _config = _configStorage.Load();
    }

    public void ReloadConfig()
    {
        _config = _configStorage.Load();
    }

    public int GetCurrentPrice(Player player)
    {
        var dx = player.Vehicle.X - _config.GarageX;
        var dy = player.Vehicle.Y - _config.GarageY;
        var distanceKm = Math.Sqrt(dx * dx + dy * dy) / 1000.0;

        var raw = _config.BaseFare + (int)Math.Round(distanceKm * _config.PricePerKm);

        return Math.Clamp(raw, _config.MinPrice, _config.MaxPrice);
    }

    public async Task<bool> TowPlayerAsync(Player player, CancellationToken cancellationToken)
    {
        if (player.Vehicle.Speed > MaxStopSpeedKmh)
        {
            await _sendMessage(player.UCID, "^1Turi sustoti, kad iskviestum vilkika.", cancellationToken);
            return false;
        }

        var price = GetCurrentPrice(player);

        if (!_economyService.RemoveMoney(player, price))
        {
            await _sendMessage(player.UCID, $"^1Nepakanka pinigu. Kaina: ^7{price}$", cancellationToken);
            return false;
        }

        await _databaseService.SavePlayerAsync(player, cancellationToken);

        await _sendCarReset(player.PLID, cancellationToken);

        await _sendMessage(player.UCID, $"^2Vilkikas atvyko! Nuskaiciuota: ^7{price}$", cancellationToken);

        return true;
    }
}