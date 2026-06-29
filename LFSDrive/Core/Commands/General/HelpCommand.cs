using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.General;

public sealed class HelpCommand : ICommand

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

    public HelpCommand(Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _sendMessage = sendMessage;
    }

    public string Name => "help";
    public string Description => "Shows available commands.";

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        await _sendMessage(player.UCID, "^2Commands: ^7!help, !id", cancellationToken);
    }
}