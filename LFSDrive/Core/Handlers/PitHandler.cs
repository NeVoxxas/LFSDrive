using LfsCruise.Core.Economy;
using LfsCruise.Core.Players;
using LfsCruise.Database;
using LfsCruise.InSim.Packets;

namespace LfsCruise.InSim.Handlers;

public sealed class PitHandler
{
    private readonly PlayerManager _playerManager;
    private readonly EconomyService _economyService;
    private readonly DatabaseService _databaseService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public PitHandler(
        PlayerManager playerManager,
        EconomyService economyService,
        DatabaseService databaseService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _playerManager = playerManager;
        _economyService = economyService;
        _databaseService = databaseService;
        _sendMessage = sendMessage;
    }

    public async Task HandleAsync(PitPacket pit, CancellationToken cancellationToken)
    {
        var player = _playerManager.GetByPlid(pit.PLID);

        if (player is null)
            return;

        const int price = 50;

        if (!_economyService.RemoveMoney(player, price))
        {
            await _sendMessage(player.UCID, "^1Nepakanka pinigu pitstopui.", cancellationToken);
            return;
        }

        await _databaseService.SavePlayerAsync(player, cancellationToken);

        await _sendMessage(
            player.UCID,
            $"^2Pitstopas pradetas. Nuskaiciuota: ^7{price}$",
            cancellationToken);
    }
}