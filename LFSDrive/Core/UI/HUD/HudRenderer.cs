using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.HUD;

public sealed class HudRenderer
{
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendButton;
    private readonly Func<byte, byte, byte, CancellationToken, Task> _deleteButtons;

    private readonly HashSet<byte> _initializedPlayers = new();
    private readonly Dictionary<byte, List<byte>> _lastVisibleClickIds = new();

    public HudRenderer(
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendButton,
        Func<byte, byte, byte, CancellationToken, Task> deleteButtons)
    {
        _sendButton = sendButton;
        _deleteButtons = deleteButtons;
    }

    public async Task RenderAsync(
        Player player,
        IReadOnlyList<HudWidget> widgets,
        CancellationToken cancellationToken)
    {
        var visibleWidgets = widgets.Where(w => w.IsVisible(player)).ToList();
        var visibleIds = visibleWidgets.Select(w => w.ClickId).ToList();

        var layoutChanged =
            !_initializedPlayers.Contains(player.UCID) ||
            !_lastVisibleClickIds.TryGetValue(player.UCID, out var lastIds) ||
            !lastIds.SequenceEqual(visibleIds);

        if (layoutChanged)
        {
            // Ištriname mygtukus tų widgetų, kurie buvo matomi, bet dabar nebe
            if (_lastVisibleClickIds.TryGetValue(player.UCID, out var previousIds))
            {
                foreach (var id in previousIds)
                {
                    if (!visibleIds.Contains(id))
                    {
                        await _deleteButtons(player.UCID, id, id, cancellationToken);
                    }
                }
            }

            await DrawFullHudAsync(player, visibleWidgets, cancellationToken);

            _initializedPlayers.Add(player.UCID);
            _lastVisibleClickIds[player.UCID] = visibleIds;
            return;
        }

        await UpdateHudTextAsync(player, visibleWidgets, cancellationToken);
    }

    public void RemovePlayer(byte ucid)
    {
        _initializedPlayers.Remove(ucid);
        _lastVisibleClickIds.Remove(ucid); // NAUJA
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