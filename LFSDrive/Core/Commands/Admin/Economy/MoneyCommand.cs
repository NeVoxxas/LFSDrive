using LfsCruise.Core.Players;
using System.Globalization;

namespace LfsCruise.Core.Commands.Admin.Economy;

public sealed class MoneyCommand : ICommand
{
    /*public enum CommandPermission
    {
        Player,
        Admin,
        Moderator,
        Developer
    }*/

    public bool AdminOnly => false;

    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public MoneyCommand(Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _sendMessage = sendMessage;
    }

    public string Name => "money";
    public string Description => "Pinigai";

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        await _sendMessage(
            player.UCID,
            $"^2Pinigai: ^7${player.Data.Money} ^2Banke ^7${player.Data.Bank}",
            cancellationToken
        );
    }
}
