using LfsCruise.Core.Players;
using LfsCruise.Core.UI;

namespace LfsCruise.Core.GPS;

public sealed class GpsService
{

    private readonly Dictionary<byte, GpsTarget> _targets = new();
    private readonly Dictionary<byte, DateTime> _lastUpdates = new();

    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendButton;
    private readonly Func<byte, byte, byte, CancellationToken, Task> _deleteButtons;

    public GpsService(
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendButton,
        Func<byte, byte, byte, CancellationToken, Task> deleteButtons)
    {
        _sendButton = sendButton;
        _deleteButtons = deleteButtons;
    }

    public void SetTarget(Player player, string name, double x, double y)
    {
        _targets[player.UCID] = new GpsTarget
        {
            Name = name,
            X = x,
            Y = y
        };
    }

    public async Task ClearTargetAsync(Player player, CancellationToken cancellationToken)
    {
        _targets.Remove(player.UCID);
        _lastUpdates.Remove(player.UCID);

        await _deleteButtons(
            player.UCID,
            ClickIds.Gps.Background,
            ClickIds.Gps.Info,
            cancellationToken);
    }

    public async Task UpdateAsync(Player player, CancellationToken cancellationToken)
    {
        if (!_targets.TryGetValue(player.UCID, out var target))
            return;

        if (_lastUpdates.TryGetValue(player.UCID, out var lastUpdate) &&
            (DateTime.UtcNow - lastUpdate).TotalSeconds < 0.5)
            return;

        _lastUpdates[player.UCID] = DateTime.UtcNow;

        var dx = target.X - player.Vehicle.X;
        var dy = target.Y - player.Vehicle.Y;

        var distance = Math.Sqrt(dx * dx + dy * dy);
        var direction = GetRelativeDirection(player.Vehicle.Heading, dx, dy);

        await DrawGpsAsync(
            player.UCID,
            target.Name,
            distance,
            direction,
            cancellationToken);
    }

    private async Task DrawGpsAsync(
        byte ucid,
        string targetName,
        double distance,
        GpsDirection direction,
        CancellationToken cancellationToken)
    {
        const byte left = 6;
        const byte top = 58;

        const byte bgWidth = 19;
        const byte bgHeight = 24;

        const byte cellWidth = 5;
        const byte cellHeight = 5;

        const byte colLeft = left + 2;
        const byte colCenter = left + 7;
        const byte colRight = left + 12;

        const byte rowTop = top + 2;
        const byte rowMiddle = top + 8;
        const byte rowBottom = top + 14;

        await _sendButton(
            ucid,
            ClickIds.Gps.Background,
            left,
            top,
            bgWidth,
            bgHeight,
            "",
            cancellationToken);

        await DrawCellAsync(
            ucid,
            ClickIds.Gps.TopLeft,
            colLeft,
            rowTop,
            cellWidth,
            cellHeight,
            direction == GpsDirection.ForwardLeft ? "\\" : "",
            cancellationToken);

        await DrawCellAsync(
            ucid,
            ClickIds.Gps.Top,
            colCenter,
            rowTop,
            cellWidth,
            cellHeight,
            direction == GpsDirection.Forward ? "|" : "",
            cancellationToken);

        await DrawCellAsync(
            ucid,
            ClickIds.Gps.TopRight,
            colRight,
            rowTop,
            cellWidth,
            cellHeight,
            direction == GpsDirection.ForwardRight ? "/" : "",
            cancellationToken);

        await DrawCellAsync(
            ucid,
            ClickIds.Gps.Left,
            colLeft,
            rowMiddle,
            cellWidth,
            cellHeight,
            direction == GpsDirection.Left ? "<" : "",
            cancellationToken);

        await DrawCellAsync(
            ucid,
            ClickIds.Gps.Center,
            colCenter,
            rowMiddle,
            cellWidth,
            cellHeight,
            "^5O",
            cancellationToken);

        await DrawCellAsync(
            ucid,
            ClickIds.Gps.Right,
            colRight,
            rowMiddle,
            cellWidth,
            cellHeight,
            direction == GpsDirection.Right ? ">" : "",
            cancellationToken);

        await DrawCellAsync(
            ucid,
            ClickIds.Gps.BottomLeft,
            colLeft,
            rowBottom,
            cellWidth,
            cellHeight,
            direction == GpsDirection.BackLeft ? "/" : "",
            cancellationToken);

        await DrawCellAsync(
            ucid,
            ClickIds.Gps.Bottom,
            colCenter,
            rowBottom,
            cellWidth,
            cellHeight,
            direction == GpsDirection.Back ? "|" : "",
            cancellationToken);

        await DrawCellAsync(
            ucid,
            ClickIds.Gps.BottomRight,
            colRight,
            rowBottom,
            cellWidth,
            cellHeight,
            direction == GpsDirection.BackRight ? "\\" : "",
            cancellationToken);

        await _sendButton(
            ucid,
            ClickIds.Gps.Info,
            left,
            (byte)(top + 17),
            32,
            6,
            $"^7{distance:0} m • {targetName}",
            cancellationToken);
    }

    private Task DrawCellAsync(
        byte ucid,
        byte clickId,
        byte left,
        byte top,
        byte width,
        byte height,
        string text,
        CancellationToken cancellationToken)
    {
        return _sendButton(
            ucid,
            clickId,
            left,
            top,
            width,
            height,
            string.IsNullOrWhiteSpace(text) ? "" : $"^7{text}",
            cancellationToken);
    }

    private static GpsDirection GetRelativeDirection(double heading, double dx, double dy)
    {
        var targetLength = Math.Sqrt(dx * dx + dy * dy);

        if (targetLength < 0.001)
            return GpsDirection.Forward;

        var targetX = dx / targetLength;
        var targetY = dy / targetLength;

        var headingRadians = HeadingToRadians(heading);

        var forwardX = Math.Sin(headingRadians);
        var forwardY = Math.Cos(headingRadians);

        var dot = forwardX * targetX + forwardY * targetY;
        var cross = forwardX * targetY - forwardY * targetX;

        var forwardThreshold = 0.45;
        var sideThreshold = 0.45;

        if (dot > forwardThreshold)
        {
            if (cross < -sideThreshold)
                return GpsDirection.ForwardRight;

            if (cross > sideThreshold)
                return GpsDirection.ForwardLeft;

            return GpsDirection.Forward;
        }

        if (dot < -forwardThreshold)
        {
            if (cross < -sideThreshold)
                return GpsDirection.BackRight;

            if (cross > sideThreshold)
                return GpsDirection.BackLeft;

            return GpsDirection.Back;
        }

        if (cross < 0)
            return GpsDirection.Right;

        return GpsDirection.Left;
    }

    private static double HeadingToRadians(double heading)
    {
        return heading * Math.PI * 2.0 / 65536.0;
    }
    private enum GpsDirection
    {
        Forward,
        ForwardRight,
        Right,
        BackRight,
        Back,
        BackLeft,
        Left,
        ForwardLeft
    }
}