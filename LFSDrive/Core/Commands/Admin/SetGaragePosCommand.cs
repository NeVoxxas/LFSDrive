using LfsCruise.Core.Players;
using LfsCruise.Core.Vehicles.Tow;

namespace LfsCruise.Core.Commands.Admin;

public sealed class SetGaragePosCommand : ICommand
{
    private readonly TowConfigStorage _storage;
    private readonly TowService _towService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public SetGaragePosCommand(
        TowConfigStorage storage,
        TowService towService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _storage = storage;
        _towService = towService;
        _sendMessage = sendMessage;
    }

    public string Name => "setgaragepos";
    public string Description => "Nustato taska, nuo kurio skaiciuojama vilkiko kaina (tavo dabartine pozicija).";
    public bool AdminOnly => true;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        var config = _storage.Load();

        config.GarageX = player.Vehicle.X;
        config.GarageY = player.Vehicle.Y;

        _storage.Save(config);
        _towService.ReloadConfig();

        await _sendMessage(
            player.UCID,
            $"^2Garazo taskas nustatytas: ^7X={config.GarageX:F1}, Y={config.GarageY:F1}",
            cancellationToken);
    }
}