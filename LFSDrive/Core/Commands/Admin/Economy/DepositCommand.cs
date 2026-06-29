using LfsCruise.Core.Economy;
using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.Admin.Economy;

public sealed class DepositCommand : ICommand
{
    /*public enum CommandPermission
    {
        Player,
        Admin,
        Moderator,
        Developer
    }*/

    public bool AdminOnly => false;

    private readonly EconomyService _economy;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public DepositCommand(EconomyService economy, Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _economy = economy;
        _sendMessage = sendMessage;
    }

    public string Name => "deposit";
    public string Description => "Deposit money to bank.";

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var amount))
        {
            await _sendMessage(player.UCID, "^1Usage: ^7!deposit <amount>", cancellationToken);
            return;
        }

        if (!_economy.Deposit(player, amount))
        {
            await _sendMessage(player.UCID, "^1You don't have enough cash.", cancellationToken);
            return;
        }

        await _sendMessage(player.UCID, $"^2Deposited: ^7€{amount}", cancellationToken);
    }
}