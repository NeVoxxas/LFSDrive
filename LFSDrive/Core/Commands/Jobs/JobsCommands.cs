using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.Jobs;

public sealed class JobsCommand : ICommand
{
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public JobsCommand(Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _sendMessage = sendMessage;
    }

    public string Name => "jobs";
    public string Description => "Rodo darbu sarasa";
    public bool AdminOnly => false;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        await _sendMessage(player.UCID, "^2Darbai: ^7taxi, delivery, bus, police", cancellationToken);
        await _sendMessage(player.UCID, "^7Naudok: ^2!joinjob taxi", cancellationToken);
    }
}