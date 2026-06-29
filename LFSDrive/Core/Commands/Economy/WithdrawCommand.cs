using LfsCruise.Core.Economy;
using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.Economy;

public sealed class WithdrawCommand : ICommand
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

    public WithdrawCommand(EconomyService economy, Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _economy = economy;
        _sendMessage = sendMessage;
    }

    public string Name => "withdraw";
    public string Description => "Withdraw money from bank.";

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var amount))
        {
            await _sendMessage(player.UCID, "^1Usage: ^7!withdraw <amount>", cancellationToken);
            return;
        }

        if (!_economy.Withdraw(player, amount))
        {
            await _sendMessage(player.UCID, "^1You don't have enough money in bank.", cancellationToken);
            return;
        }

        await _sendMessage(player.UCID, $"^2Withdrawn: ^7€{amount}", cancellationToken);
    }
}