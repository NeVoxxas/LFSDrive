using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.HUD;

public sealed class HudRenderer
{
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendButton;

    private readonly HashSet<byte> _initializedPlayers = new();

    public HudRenderer(
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendButton)
    {
        _sendButton = sendButton;
    }

    public async Task RenderAsync(
        Player player,
        IReadOnlyList<HudWidget> widgets,
        CancellationToken cancellationToken)
    {
        if (!_initializedPlayers.Contains(player.UCID))
        {
            await DrawFullHudAsync(player, widgets, cancellationToken);
            _initializedPlayers.Add(player.UCID);
            return;
        }

        await UpdateHudTextAsync(player, widgets, cancellationToken);
    }

    public void RemovePlayer(byte ucid)
    {
        _initializedPlayers.Remove(ucid);
    }

    private async Task DrawFullHudAsync(
        Player player,
        IReadOnlyList<HudWidget> widgets,
        CancellationToken cancellationToken)
    {
        var left = HudLayout.StartLeft;

        foreach (var widget in widgets)
        {
            await _sendButton(
                player.UCID,
                widget.ClickId,
                left,
                HudLayout.Top,
                widget.Width,
                HudLayout.Height,
                widget.GetText(player),
                cancellationToken);

            left += (byte)(widget.Width + HudLayout.Spacing);
        }
    }

    private async Task UpdateHudTextAsync(
        Player player,
        IReadOnlyList<HudWidget> widgets,
        CancellationToken cancellationToken)
    {
        foreach (var widget in widgets)
        {
            await _sendButton(
                player.UCID,
                widget.ClickId,
                0,
                0,
                0,
                0,
                widget.GetText(player),
                cancellationToken);
        }
    }
}