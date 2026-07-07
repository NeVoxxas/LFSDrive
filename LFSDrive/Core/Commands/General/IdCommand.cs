using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.General;

public sealed class IdCommand : ICommand
{
    public bool AdminOnly => false;

    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public IdCommand(Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _sendMessage = sendMessage;
    }

    public string Name => "id";
    public string Description => "Shows your connection ID.";

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        await _sendMessage(player.UCID, $"^2Your UCID: ^7{player.UCID} ^2Rank: ^7{player.Rank}", cancellationToken);
    }
}