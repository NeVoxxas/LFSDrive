using LfsCruise.Core.Jobs.Taxi;
using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.Jobs;

public sealed class TaxiFareCommand : ICommand
{
    private readonly TaxiPointStorage _storage;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public TaxiFareCommand(
        TaxiPointStorage storage,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _storage = storage;
        _sendMessage = sendMessage;
    }

    public string Name => "taxifare";

    public string Description => "Keicia taxi tarifus";

    public bool AdminOnly => true;

    public async Task ExecuteAsync(
        Player player,
        string[] args,
        CancellationToken cancellationToken)
    {
        var config = _storage.Load();

        if (args.Length == 0)
        {
            await ShowFareAsync(player, config, cancellationToken);
            return;
        }

        if (args.Length != 2)
        {
            await _sendMessage(
                player.UCID,
                "^1Naudok: ^7!taxifare basefare 100 ^1arba ^7!taxifare priceperkm 300",
                cancellationToken);

            return;
        }

        var field = args[0].ToLower();

        if (!int.TryParse(args[1], out var value) || value < 0)
        {
            await _sendMessage(
                player.UCID,
                "^1Reiksme turi buti teigiamas skaicius.",
                cancellationToken);

            return;
        }

        switch (field)
        {
            case "basefare":
                config.Fare.BaseFare = value;
                break;

            case "priceperkm":
                config.Fare.PricePerKm = value;
                break;

            case "minreward":
                config.Fare.MinReward = value;
                break;

            case "maxreward":
                config.Fare.MaxReward = value;
                break;

            default:
                await _sendMessage(
                    player.UCID,
                    "^1Nezinomas tarifas. Galimi: ^7basefare, priceperkm, minreward, maxreward",
                    cancellationToken);

                return;
        }

        if (config.Fare.MaxReward < config.Fare.MinReward)
        {
            await _sendMessage(
                player.UCID,
                "^1maxreward negali buti mazesnis uz minreward.",
                cancellationToken);

            return;
        }

        _storage.Save(config);

        await _sendMessage(
            player.UCID,
            $"^2Taxi tarifas pakeistas: ^7{field} = {value}",
            cancellationToken);
    }

    private async Task ShowFareAsync(
        Player player,
        TaxiPointConfig config,
        CancellationToken cancellationToken)
    {
        await _sendMessage(
            player.UCID,
            $"^3Taxi tarifai: ^7basefare={config.Fare.BaseFare}, priceperkm={config.Fare.PricePerKm}, minreward={config.Fare.MinReward}, maxreward={config.Fare.MaxReward}",
            cancellationToken);
    }
}