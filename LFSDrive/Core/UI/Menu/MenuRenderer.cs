using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.Menu;

public sealed class MenuRenderer
{
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendPanel;
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendLabel;
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendItem;
    private readonly Func<byte, byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendInputButton;
    private readonly Func<byte, byte, byte, CancellationToken, Task> _deleteButtons;
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendCategoryHeader;

    private const byte Left = 72;
    private const byte PanelWidth = 65;
    private const byte TitleTop = 55;
    private const byte ListTop = 61;
    private const byte LineHeight = 6;

    private const byte WideLeft = 55;
    private const byte WidePanelWidth = 90;
    private const byte WideTitleTop = 40;
    private const byte WideLineHeight = 6;
    private const byte WideColumnGap = 3;

    public MenuRenderer(
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendPanel,
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendLabel,
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendItem,
        Func<byte, byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendInputButton,
        Func<byte, byte, byte, CancellationToken, Task> deleteButtons,
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendCategoryHeader)
    {
        _sendPanel = sendPanel;
        _sendLabel = sendLabel;
        _sendItem = sendItem;
        _sendInputButton = sendInputButton;
        _deleteButtons = deleteButtons;
        _sendCategoryHeader = sendCategoryHeader;
    }

    public async Task RenderAsync(Player player, MenuPage page, CancellationToken cancellationToken)
    {
        var context = new MenuContext { Player = player };
        var buttons = page.GetButtons(context);

        var isCategorized = buttons.Any(b => b.Category is not null);

        if (isCategorized)
        {
            await RenderCategorizedAsync(player, page, buttons, cancellationToken);
        }
        else
        {
            await RenderLegacyAsync(player, page, buttons, cancellationToken);
        }
    }

    // ============================================================
    // SENAS STILIUS - Shop, Bank, Jobs, Regitra, Market...
    // ============================================================
    private async Task RenderLegacyAsync(
        Player player,
        MenuPage page,
        IReadOnlyList<MenuButton> buttons,
        CancellationToken cancellationToken)
    {
        var contentRows = buttons.Count + 2;
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

            top += button.Height;
        }

        const byte closeWidth = 24;
        var closeLeft = (byte)(Left + PanelWidth - closeWidth - 3);

        await _sendItem(
            player.UCID, ClickIds.Menu.Close,
            closeLeft, (byte)(top + 3), closeWidth, LineHeight,
            "^1Uzdaryti", cancellationToken);
    }

    // ============================================================
    // NAUJAS STILIUS - kategorizuotas, 2 stulpelių (MainMenuPage)
    // ============================================================
    private async Task RenderCategorizedAsync(
        Player player,
        MenuPage page,
        IReadOnlyList<MenuButton> buttons,
        CancellationToken cancellationToken)
    {
        var categories = buttons
            .Where(b => b.Category is not null)
            .GroupBy(b => b.Category!)
            .Select(g => new
            {
                Title = g.Key,
                Left = g.Where(b => b.Column == MenuColumn.Left).ToList(),
                Right = g.Where(b => b.Column == MenuColumn.Right).ToList()
            })
            .ToList();

        var columnWidth = (byte)((WidePanelWidth - WideColumnGap * 3) / 2);
        var leftColumnLeft = (byte)(WideLeft + WideColumnGap);
        var rightColumnLeft = (byte)(leftColumnLeft + columnWidth + WideColumnGap);

        var totalRows = 1;
        foreach (var category in categories)
        {
            totalRows += 1;
            totalRows += Math.Max(category.Left.Count, category.Right.Count);
        }
        totalRows += 1;

        var panelHeight = (byte)Math.Clamp(totalRows * WideLineHeight + 10, 40, 200);

        await _sendPanel(
            player.UCID, ClickIds.Menu.Background,
            WideLeft, (byte)(WideTitleTop - 4), WidePanelWidth, panelHeight,
            "", cancellationToken);

        var top = WideTitleTop;

        await _sendLabel(
            player.UCID, ClickIds.Menu.Title,
            WideLeft, top, WidePanelWidth, WideLineHeight,
            $"^3{page.Title}", cancellationToken);

        top += WideLineHeight;

        byte headerClickId = ClickIds.MenuCategory.Start;

        foreach (var category in categories)
        {
            if (headerClickId > ClickIds.MenuCategory.End)
                break;

            await _sendCategoryHeader(
                player.UCID, headerClickId++,
                WideLeft, top, WidePanelWidth, WideLineHeight,
                $"^3{category.Title}", cancellationToken);

            top += WideLineHeight;

            var rowCount = Math.Max(category.Left.Count, category.Right.Count);

            for (var i = 0; i < rowCount; i++)
            {
                if (i < category.Left.Count)
                {
                    await RenderCellAsync(
                        player, category.Left[i], leftColumnLeft, top, columnWidth, cancellationToken);
                }

                if (i < category.Right.Count)
                {
                    await RenderCellAsync(
                        player, category.Right[i], rightColumnLeft, top, columnWidth, cancellationToken);
                }

                top += WideLineHeight;
            }
        }

        await _sendItem(
            player.UCID, ClickIds.Menu.Close,
            WideLeft, top, WidePanelWidth, WideLineHeight,
            "^1Uzdaryti", cancellationToken);
    }

    private Task RenderCellAsync(
        Player player,
        MenuButton button,
        byte left,
        byte top,
        byte width,
        CancellationToken cancellationToken)
    {
        var text = FormatText(button);

        if (button.TypeIn > 0)
        {
            return _sendInputButton(
                player.UCID, button.ClickId, left, top, width, WideLineHeight,
                button.TypeIn, text, cancellationToken);
        }

        if (button.Enabled)
        {
            return _sendItem(
                player.UCID, button.ClickId, left, top, width, WideLineHeight,
                text, cancellationToken);
        }

        return _sendLabel(
            player.UCID, button.ClickId, left, top, width, WideLineHeight,
            text, cancellationToken);
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
        for (byte id = ClickIds.Menu.Background; id <= ClickIds.Regitra.MenuButton; id++)
        {
            await _deleteButtons(player.UCID, id, id, cancellationToken);
        }

        await _deleteButtons(
            player.UCID, ClickIds.Content.Start, ClickIds.Content.End, cancellationToken);

        await _deleteButtons(
            player.UCID, ClickIds.MenuCategory.Start, ClickIds.MenuCategory.End, cancellationToken);
    }
}
