using LfsCruise.Core.Players;
using LfsCruise.Core.Vehicles.Market;

namespace LfsCruise.Core.Commands.Vehicles;

public sealed class SellCarCommand : ICommand
{
    private readonly MarketService _marketService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public SellCarCommand(MarketService marketService, Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _marketService = marketService;
        _sendMessage = sendMessage;
    }

    public string Name => "parduoti";
    public string Description => "Iskelia tavo automobili i Auto turgu.";
    public bool AdminOnly => false;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var price) || price <= 0)
        {
            await _sendMessage(player.UCID, "^1Naudok: ^7!parduoti <kaina>", cancellationToken);
            return;
        }

        var result = await _marketService.CreateListingAsync(player, price, cancellationToken);

        await _sendMessage(
            player.UCID,
            result.Success ? $"^2{result.Message}" : $"^1{result.Message}",
            cancellationToken);
    }
}