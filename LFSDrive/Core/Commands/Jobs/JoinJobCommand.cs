using LfsCruise.Core.Jobs;
using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.Jobs;

public sealed class JoinJobCommand : ICommand
{
    private readonly JobManager _jobManager;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public JoinJobCommand(
        JobManager jobManager,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _jobManager = jobManager;
        _sendMessage = sendMessage;
    }

    public string Name => "joinjob";
    public string Description => "Isidarbinti i darba";
    public bool AdminOnly => false;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            await _sendMessage(player.UCID, "^1Naudok: ^7!joinjob taxi", cancellationToken);
            return;
        }

        if (!TryParseJob(args[0], out var job))
        {
            await _sendMessage(player.UCID, "^1Tokio darbo nera. Darbai: ^7taxi, delivery, bus", cancellationToken);
            return;
        }

        await _jobManager.StartJobAsync(player, job, cancellationToken);
    }

    private static bool TryParseJob(string value, out JobType job)
    {
        job = value.ToLower() switch
        {
            "taxi" => JobType.Taxi,
            "delivery" => JobType.Delivery,
            "bus" => JobType.Bus,
            _ => JobType.None
        };

        return job != JobType.None;
    }
}