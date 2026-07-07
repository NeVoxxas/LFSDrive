using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.HUD;

public sealed class HudRenderer
{
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendButton;
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendLabel;
    private readonly Func<byte, byte, byte, CancellationToken, Task> _deleteButtons;

    private readonly HashSet<byte> _initializedPlayers = new();
    private readonly Dictionary<byte, List<byte>> _lastVisibleClickIds = new();
    private readonly Dictionary<byte, Dictionary<byte, bool>> _lastInteractiveState = new();

    public HudRenderer(
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendButton,
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendLabel,
        Func<byte, byte, byte, CancellationToken, Task> deleteButtons)
    {
        _sendButton = sendButton;
        _sendLabel = sendLabel;
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
            _lastInteractiveState[player.UCID] = visibleWidgets.ToDictionary(w => w.ClickId, w => w.IsInteractive(player));
            return;
        }

        await UpdateHudAsync(player, visibleWidgets, cancellationToken);
    }

    public void RemovePlayer(byte ucid)
    {
        _initializedPlayers.Remove(ucid);
        _lastVisibleClickIds.Remove(ucid);
        _lastInteractiveState.Remove(ucid);
    }

    private async Task DrawFullHudAsync(
        Player player,
        IReadOnlyList<HudWidget> widgets,
        CancellationToken cancellationToken)
    {
        var left = HudLayout.StartLeft;

        foreach (var widget in widgets)
        {
            await SendWidgetAsync(player, widget, left, cancellationToken);
            left += (byte)(widget.Width + HudLayout.Spacing);
        }
    }

    private async Task UpdateHudAsync(
        Player player,
        IReadOnlyList<HudWidget> widgets,
        CancellationToken cancellationToken)
    {
        var previousState = _lastInteractiveState.TryGetValue(player.UCID, out var state)
            ? state
            : new Dictionary<byte, bool>();

        var newState = new Dictionary<byte, bool>();
        var left = HudLayout.StartLeft;

        foreach (var widget in widgets)
        {
            var isInteractive = widget.IsInteractive(player);
            newState[widget.ClickId] = isInteractive;

            var interactivityChanged =
                !previousState.TryGetValue(widget.ClickId, out var wasInteractive) ||
                wasInteractive != isInteractive;

            if (interactivityChanged)
            {
                await SendWidgetAsync(player, widget, left, cancellationToken);
            }
            else
            {
                await SendTextOnlyAsync(player, widget, cancellationToken);
            }

            left += (byte)(widget.Width + HudLayout.Spacing);
        }

        _lastInteractiveState[player.UCID] = newState;
    }

    private Task SendWidgetAsync(Player player, HudWidget widget, byte left, CancellationToken cancellationToken)
    {
        var send = widget.IsInteractive(player) ? _sendButton : _sendLabel;

        return send(
            player.UCID, widget.ClickId, left, HudLayout.Top, widget.Width, HudLayout.Height,
            widget.GetText(player), cancellationToken);
    }

    private Task SendTextOnlyAsync(Player player, HudWidget widget, CancellationToken cancellationToken)
    {
        return _sendButton(player.UCID, widget.ClickId, 0, 0, 0, 0, widget.GetText(player), cancellationToken);
    }
}