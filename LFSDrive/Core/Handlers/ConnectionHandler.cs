using LfsCruise.Core.Events;
using LfsCruise.Core.Jobs;
using LfsCruise.Core.Players;
using LfsCruise.Core.UI.HUD;
using LfsCruise.Core.Vehicles;
using LfsCruise.Core.Vehicles.Mods;
using LfsCruise.Core.Vehicles.Shop;
using LfsCruise.Database;
using LfsCruise.InSim.Packets;

public sealed class ConnectionHandler
{
    private readonly PlayerManager _playerManager;
    private readonly DatabaseService _databaseService;
    private readonly EventBus _eventBus;
    private readonly HudManager _hudManager;
    private readonly ModNameService _modNameService;
    private readonly VehicleOwnershipService _vehicleOwnershipService;
    private readonly VehicleShopService _vehicleShopService;
    private readonly JobService _jobService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;
    private readonly Func<string, CancellationToken, Task> _sendHostCommand;
    private readonly Func<byte, byte, CancellationToken, Task> _sendJoinReply;

    public ConnectionHandler(
        PlayerManager playerManager,
        DatabaseService databaseService,
        EventBus eventBus,
        HudManager hudManager,
        ModNameService modNameService,
        VehicleOwnershipService vehicleOwnershipService,
        VehicleShopService vehicleShopService,
        JobService jobService,
        Func<byte, string, CancellationToken, Task> sendMessage,
        Func<string, CancellationToken, Task> sendHostCommand,
        Func<byte, byte, CancellationToken, Task> sendJoinReply)
    {
        _playerManager = playerManager;
        _databaseService = databaseService;
        _eventBus = eventBus;
        _hudManager = hudManager;
        _modNameService = modNameService;
        _vehicleOwnershipService = vehicleOwnershipService;
        _vehicleShopService = vehicleShopService;
        _jobService = jobService;
        _sendMessage = sendMessage;
        _sendHostCommand = sendHostCommand;
        _sendJoinReply = sendJoinReply;
    }

    public Task HandleConnectedAsync(NcnPacket ncn, CancellationToken cancellationToken)
    {
        return _eventBus.PublishAsync(ncn, cancellationToken);
    }

    public async Task HandleDisconnectedAsync(CnlPacket cnl, CancellationToken cancellationToken)
    {
        var player = _playerManager.Get(cnl.UCID);

        if (player is not null)
        {
            await _databaseService.SavePlayerAsync(player, cancellationToken);
        }

        _hudManager.RemovePlayer(cnl.UCID);
        _playerManager.Remove(cnl.UCID);
    }

    public async Task HandleNewPlayerAsync(NplPacket npl, CancellationToken cancellationToken)
    {
        // join request (ISF_REQ_JOIN) - automatiškai patvirtiname, kad žaidėjas
        // iškart atsispawnintų garaže, be "prisijungti" ekrano.
        if (npl.IsJoinRequest)
        {
            await _sendJoinReply(npl.UCID, JrrPacket.ActionSpawn, cancellationToken);
            Console.WriteLine($"JOIN REQUEST | UCID {npl.UCID} -> auto JRR_SPAWN");
            return;
        }

        var player = _playerManager.Get(npl.UCID);

        if (player is null)
            return;

        player.PLID = npl.PLID;
        player.CarName = npl.CarCode;
        var vehicle = _vehicleShopService.GetVehicleByCode(npl.CarCode);

        string displayName;
        if (vehicle is not null)
        {
            displayName = vehicle.DisplayName;
        }
        else if (npl.IsMod)
        {
            displayName = await _modNameService.ResolveNameAsync(npl.SkinId, cancellationToken);
        }
        else
        {
            displayName = npl.CarCode;
        }

        var ownsVehicle = await _vehicleOwnershipService.OwnsVehicleAsync(player, npl.CarCode, cancellationToken);

        if (!ownsVehicle)
        {
            await _sendMessage(player.UCID, $"^1Tu nesi nusipirkes sios masinos: ^7{displayName}", cancellationToken);
            //await _sendHostCommand($"/spec {player.Username}", cancellationToken);
            return;
        }

        if (_jobService.TryGetRequiredJob(npl.CarCode, out var requiredJob) &&
            _jobService.GetJob(player) != requiredJob)
        {
            await _sendMessage(
                player.UCID,
                "^1Sia masina gali naudoti tik budedamas atitinkamoje pamainoje. ^7Naudok: !joinjob delivery",
                cancellationToken);

            await _sendHostCommand($"/spec {player.Username}", cancellationToken);
            return;
        }

        Console.WriteLine($"PLAYER CAR: {npl.CarCode}");
        Console.WriteLine($"NPL | {player.Username} -> PLID {player.PLID}, Car {player.CarName}");
    }
}