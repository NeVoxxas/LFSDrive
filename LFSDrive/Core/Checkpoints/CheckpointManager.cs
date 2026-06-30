using LfsCruise.Core.Players;

namespace LfsCruise.Core.Checkpoints;

public sealed class CheckpointManager
{
    private readonly Dictionary<byte, Checkpoint> _activeCheckpoints = new();

    public void SetCheckpoint(
        Player player,
        string key,
        double x,
        double y,
        double radius = 10.0)
    {
        _activeCheckpoints[player.UCID] = new Checkpoint
        {
            Ucid = player.UCID,
            Key = key,
            X = x,
            Y = y,
            Radius = radius
        };
    }

    public void ClearCheckpoint(Player player)
    {
        _activeCheckpoints.Remove(player.UCID);
    }

    public void ClearCheckpoint(byte ucid)
    {
        _activeCheckpoints.Remove(ucid);
    }

    public bool HasCheckpoint(Player player)
    {
        return _activeCheckpoints.ContainsKey(player.UCID);
    }

    public bool IsInsideCheckpoint(Player player, out Checkpoint? checkpoint)
    {
        checkpoint = null;

        if (!_activeCheckpoints.TryGetValue(player.UCID, out var activeCheckpoint))
            return false;

        var dx = player.Vehicle.X - activeCheckpoint.X;
        var dy = player.Vehicle.Y - activeCheckpoint.Y;

        var distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance > activeCheckpoint.Radius)
            return false;

        checkpoint = activeCheckpoint;
        return true;
    }
}