using LfsCruise.Core.Players;

namespace LfsCruise.Core.Jobs;

public sealed class JobService
{
    private readonly Dictionary<byte, JobType> _playerJobs = new();

    public JobType GetJob(Player player)
    {
        return _playerJobs.TryGetValue(player.UCID, out var job)
            ? job
            : JobType.None;
    }

    public bool HasJob(Player player)
    {
        return GetJob(player) != JobType.None;
    }

    public bool SetJob(Player player, JobType job)
    {
        if (job == JobType.None)
            return false;

        _playerJobs[player.UCID] = job;
        return true;
    }

    public void LeaveJob(Player player)
    {
        _playerJobs.Remove(player.UCID);
    }

    public void RemovePlayer(byte ucid)
    {
        _playerJobs.Remove(ucid);
    }
}