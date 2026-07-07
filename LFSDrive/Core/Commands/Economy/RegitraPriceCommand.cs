using LfsCruise.Core.Players;
using LfsCruise.Core.Vehicles.Regitra;

namespace LfsCruise.Core.Commands.Admin;

public sealed class RegitraPriceCommand : ICommand
{
    private readonly RegitraConfigStorage _storage;
    private readonly RegitraService _regitraService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public RegitraPriceCommand(
        RegitraConfigStorage storage,
        RegitraService regitraService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _storage = storage;
        _regitraService = regitraService;
        _sendMessage = sendMessage;
    }

    public string Name => "regitraprice";
    public string Description => "Keicia Regitros kainas.";
    public bool AdminOnly => true;

    public AdminRank RequiredRank => AdminRank.Moderator;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        var config = _storage.Load();

        if (args.Length == 0)
        {
            await _sendMessage(
                player.UCID,
                $"^3Regitros kainos: ^7plate={config.PlatePrice}, platechange={config.PlateChangePrice}, insurance={config.InsurancePrice}, inspection={config.InspectionPrice}",
                cancellationToken);
            return;
        }

        if (args.Length != 2)
        {
            await _sendMessage(
                player.UCID,
                "^1Naudok: ^7!regitraprice plate 500 ^1arba ^7!regitraprice insurance 300",
                cancellationToken);
            return;
        }

        var field = args[0].ToLower();

        if (!int.TryParse(args[1], out var value) || value < 0)
        {
            await _sendMessage(player.UCID, "^1Reiksme turi buti teigiamas skaicius.", cancellationToken);
            return;
        }

        switch (field)
        {
            case "plate":
                config.PlatePrice = value;
                break;

            case "platechange":
                config.PlateChangePrice = value;
                break;

            case "insurance":
                config.InsurancePrice = value;
                break;

            case "inspection":
                config.InspectionPrice = value;
                break;

            default:
                await _sendMessage(
                    player.UCID,
                    "^1Nezinomas laukas. Galimi: ^7plate, platechange, insurance, inspection",
                    cancellationToken);
                return;
        }

        _storage.Save(config);
        _regitraService.ReloadConfig();

        await _sendMessage(player.UCID, $"^2Regitros kaina pakeista: ^7{field} = {value}", cancellationToken);
    }
}