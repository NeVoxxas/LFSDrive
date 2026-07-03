using LfsCruise.Core.Players;

namespace LfsCruise.Core.Jobs;

public sealed class JobService
{
    private readonly Dictionary<byte, JobType> _playerJobs = new();
    private readonly Dictionary<byte, string> _statusTexts = new();
    private readonly Dictionary<string, JobType> _jobVehicles = new(StringComparer.OrdinalIgnoreCase);

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
        _statusTexts.Remove(player.UCID);
    }

    public void RemovePlayer(byte ucid)
    {
        _playerJobs.Remove(ucid);
        _statusTexts.Remove(ucid);
    }

    public void SetStatus(Player player, string text)
    {
        _statusTexts[player.UCID] = text;
    }

    public void ClearStatus(Player player)
    {
        _statusTexts.Remove(player.UCID);
    }

    public string GetStatus(Player player)
    {
        return _statusTexts.TryGetValue(player.UCID, out var text) ? text : string.Empty;
    }
    public void RegisterJobVehicle(string carCode, JobType requiredJob)
    {
        _jobVehicles[carCode] = requiredJob;
    }

    public bool TryGetRequiredJob(string carCode, out JobType requiredJob)
    {
        return _jobVehicles.TryGetValue(carCode, out requiredJob);
    }


}