using LfsCruise.Core.Jobs;
using LfsCruise.Core.Jobs.Police;
using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.Police;

public sealed class ChaseCommand : ICommand
{
    private const double MaxStopSpeedKmh = 1.0;

    private readonly PlayerManager _playerManager;
    private readonly JobService _jobService;
    private readonly PursuitService _pursuitService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public ChaseCommand(
        PlayerManager playerManager,
        JobService jobService,
        PursuitService pursuitService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _playerManager = playerManager;
        _jobService = jobService;
        _pursuitService = pursuitService;
        _sendMessage = sendMessage;
    }

    public string Name => "chase";
    public string Description => "[LAIKINA] Pradeda gaudyti nurodyta zaideja - kol nera radaro meniu.";
    public bool AdminOnly => false;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (_jobService.GetJob(player) != JobType.Police)
        {
            await _sendMessage(player.UCID, "^1Tik policijos pareigunai gali naudoti sia komanda.", cancellationToken);
            return;
        }

        if (args.Length < 1)
        {
            await _sendMessage(player.UCID, "^1Naudok: ^7!chase <username>", cancellationToken);
            return;
        }

        /*
        if (player.Vehicle.Speed > MaxStopSpeedKmh)
        {
            await _sendMessage(player.UCID, "^1Turi sustoti, kad galetum pradeti gaudyma.", cancellationToken);
            return;
        }
        */

        var target = _playerManager.Players.FirstOrDefault(
            p => p.Username.Equals(args[0], StringComparison.OrdinalIgnoreCase));

        if (target is null)
        {
            await _sendMessage(player.UCID, $"^1Zaidejas nerastas: ^7{args[0]}", cancellationToken);
            return;
        }

        /*
        if (target.UCID == player.UCID)
        {
            await _sendMessage(player.UCID, "^1Negali gaudyti saves.", cancellationToken);
            return;
        }

        if (_jobService.GetJob(target) == JobType.Police)
        {
            await _sendMessage(player.UCID, "^1Negalima gaudyti kito pareiguno.", cancellationToken);
            return;
        }

       if (target.Vehicle.Speed > MaxStopSpeedKmh)
        {
            await _sendMessage(player.UCID, "^1Taikinys turi buti sustojes.", cancellationToken);
            return;
        }
        */

        await _pursuitService.StartPursuitAsync(player, target, cancellationToken);
    }
}