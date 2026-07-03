using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.Menu;

public sealed class MenuRenderer
{
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendPanel;
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendLabel;
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendItem;
    private readonly Func<byte, byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendInputButton;
    private readonly Func<byte, byte, byte, CancellationToken, Task> _deleteButtons;

    private const byte Left = 72;
    private const byte PanelWidth = 65; // buvo 108
    private const byte TitleTop = 55;
    private const byte ListTop = 61;
    private const byte LineHeight = 6;

    public MenuRenderer(
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendPanel,
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendLabel,
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendItem,
        Func<byte, byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendInputButton,
        Func<byte, byte, byte, CancellationToken, Task> deleteButtons)
    {
        _sendPanel = sendPanel;
        _sendLabel = sendLabel;
        _sendItem = sendItem;
        _sendInputButton = sendInputButton;
        _deleteButtons = deleteButtons;
    }

    public async Task RenderAsync(Player player, MenuPage page, CancellationToken cancellationToken)
    {
        var context = new MenuContext { Player = player };
        var buttons = page.GetButtons(context);

        var contentRows = buttons.Count + 2; // + antraštė + Uzdaryti
        var panelHeight = (byte)Math.Clamp(contentRows * LineHeight + 8, 40, 170);

        await _sendPanel(
            player.UCID, ClickIds.Menu.Background,
            (byte)(Left - 3), (byte)(TitleTop - 5), PanelWidth, panelHeight,
            "", cancellationToken);

        await _sendLabel(
            player.UCID, ClickIds.Menu.Title,
            Left, TitleTop, PanelWidth, LineHeight,
            $"^3{page.Title}", cancellationToken);

        var top = ListTop;

        foreach (var button in buttons)
        {
            var text = FormatText(button);

            if (button.TypeIn > 0)
            {
                await _sendInputButton(
                    player.UCID, button.ClickId, Left, top, button.Width, button.Height,
                    button.TypeIn, text, cancellationToken);
            }
            else if (button.Enabled)
            {
                await _sendItem(
                    player.UCID, button.ClickId, Left, top, button.Width, button.Height,
                    text, cancellationToken);
            }
            else
            {
                await _sendLabel(
                    player.UCID, button.ClickId, Left, top, button.Width, button.Height,
                    text, cancellationToken);
            }

            top += button.Height; // pašalintas +1 tarpas, tampresnis sąrašas
        }

        const byte closeWidth = 24; // buvo 30
        var closeLeft = (byte)(Left + PanelWidth - closeWidth - 3);

        await _sendItem(
            player.UCID, ClickIds.Menu.Close,
            closeLeft, (byte)(top + 3), closeWidth, LineHeight,
            "^1Uzdaryti", cancellationToken);
    }

    private static string FormatText(MenuButton button)
    {
        if (button.ClickId == ClickIds.Menu.Back)
            return button.Text;

        if (!button.Enabled)
            return button.Text;

        return $"^7>> {button.Text}";
    }

    public async Task CloseAsync(Player player, CancellationToken cancellationToken)
    {
        for (byte id = ClickIds.Menu.Background; id <= ClickIds.Jobs.Leave; id++)
        {
            await _deleteButtons(player.UCID, id, id, cancellationToken);
        }
    }
}