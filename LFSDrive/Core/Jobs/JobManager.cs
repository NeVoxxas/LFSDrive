using LfsCruise.Core.Checkpoints;
using LfsCruise.Core.Economy;
using LfsCruise.Core.GPS;
using LfsCruise.Core.Players;
using LfsCruise.Database;

namespace LfsCruise.Core.Jobs;

public sealed class JobManager
{
    private readonly JobService _jobService;
    private readonly CheckpointManager _checkpointManager;
    private readonly GpsService _gpsService;
    private readonly EconomyService _economyService;
    private readonly DatabaseService _databaseService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendButton;
    private readonly Func<byte, byte, byte, CancellationToken, Task> _deleteButtons;

    private readonly Dictionary<JobType, IJob> _jobs = new();

    public JobManager(
        JobService jobService,
        CheckpointManager checkpointManager,
        GpsService gpsService,
        EconomyService economyService,
        DatabaseService databaseService,
        Func<byte, string, CancellationToken, Task> sendMessage,
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendButton,
        Func<byte, byte, byte, CancellationToken, Task> deleteButtons)
    {
        _jobService = jobService;
        _checkpointManager = checkpointManager;
        _gpsService = gpsService;
        _economyService = economyService;
        _databaseService = databaseService;
        _sendMessage = sendMessage;
        _sendButton = sendButton;
        _deleteButtons = deleteButtons;
    }

    public void Register(IJob job)
    {
        _jobs[job.Type] = job;
    }

    public async Task StartJobAsync(Player player, JobType jobType, CancellationToken cancellationToken)
    {
        if (!_jobs.TryGetValue(jobType, out var job))
        {
            await _sendMessage(player.UCID, "^1Sis darbas dar neparuostas.", cancellationToken);
            return;
        }

        if (_jobService.HasJob(player))
        {
            await _sendMessage(player.UCID, "^1Tu jau turi darba. Pirma naudok: ^7!leavejob", cancellationToken);
            return;
        }

        _jobService.SetJob(player, jobType);

        await job.OnStartAsync(CreateContext(player), cancellationToken);
    }

    public async Task StopJobAsync(Player player, CancellationToken cancellationToken)
    {
        var jobType = _jobService.GetJob(player);

        if (jobType == JobType.None)
        {
            await _sendMessage(player.UCID, "^1Tu neturi darbo.", cancellationToken);
            return;
        }

        if (_jobs.TryGetValue(jobType, out var job))
            await job.OnStopAsync(CreateContext(player), cancellationToken);

        _jobService.LeaveJob(player);
    }

    public async Task OnPlayerMoveAsync(Player player, CancellationToken cancellationToken)
    {
        var jobType = _jobService.GetJob(player);

        if (jobType == JobType.None)
            return;

        if (_jobs.TryGetValue(jobType, out var job))
            await job.OnPlayerMoveAsync(CreateContext(player), cancellationToken);
    }

    public void RemovePlayer(byte ucid)
    {
        _jobService.RemovePlayer(ucid);
    }

    private JobContext CreateContext(Player player)
    {
        return new JobContext
        {
            Player = player,
            JobService = _jobService,
            CheckpointManager = _checkpointManager,
            GpsService = _gpsService,
            EconomyService = _economyService,
            DatabaseService = _databaseService,
            SendMessage = _sendMessage,
            SendButton = _sendButton,
            DeleteButtons = _deleteButtons
        };
    }
}