using LfsCruise.Core.Players;
using LfsCruise.Core.Vehicles.Demand;
using LfsCruise.Core.Vehicles.Shop;

namespace LfsCruise.Core.Commands.Admin;

public sealed class VehicleDemandCommand : ICommand
{
    private readonly VehicleShopService _shopService;
    private readonly VehicleDemandService _demandService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public VehicleDemandCommand(
        VehicleShopService shopService,
        VehicleDemandService demandService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _shopService = shopService;
        _demandService = demandService;
        _sendMessage = sendMessage;
    }

    public string Name => "demand";
    public string Description => "Rodo automobilio paklausos busena (savininku sk. / kaina).";
    public bool AdminOnly => true;
    public AdminRank RequiredRank => AdminRank.Moderator;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 1)
        {
            await _sendMessage(player.UCID, "^1Naudok: ^7!demand <carCode>", cancellationToken);
            return;
        }

        var carCode = args[0];
        var vehicle = _shopService.GetVehicleByCode(carCode);

        if (vehicle is null)
        {
            await _sendMessage(player.UCID, $"^1Nerasta parduotuveje: ^7{carCode}", cancellationToken);
            return;
        }

        var ownerCount = _demandService.GetOwnerCount(carCode);
        var multiplier = _demandService.GetCurrentMultiplier(carCode);
        var currentPrice = _demandService.GetCurrentPrice(vehicle);

        await _sendMessage(
            player.UCID,
            $"^7{vehicle.DisplayName}: savininku ^3{ownerCount} ^7| baze ^2{vehicle.Price}$ ^7-> dabar ^3{currentPrice}$ ^7(x{multiplier:0.00})",
            cancellationToken);
    }
}
