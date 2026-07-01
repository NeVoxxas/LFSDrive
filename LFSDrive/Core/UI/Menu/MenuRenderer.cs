using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.Menu;

public sealed class MenuRenderer
{
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendButton;
    private readonly Func<byte, byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendInputButton;
    private readonly Func<byte, byte, byte, CancellationToken, Task> _deleteButtons;

    public MenuRenderer(
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendButton,
        Func<byte, byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendInputButton,
        Func<byte, byte, byte, CancellationToken, Task> deleteButtons)
    {
        _sendButton = sendButton;
        _sendInputButton = sendInputButton;
        _deleteButtons = deleteButtons;
    }

    public async Task RenderAsync(Player player, MenuPage page, CancellationToken cancellationToken)
    {
        await _sendButton(player.UCID, ClickIds.Menu.Background, 65, 30, 70, 80, "", cancellationToken);

        await _sendButton(player.UCID, ClickIds.Menu.Title, 75, 35, 50, 6, $"^2{page.Title}", cancellationToken);

        await _sendButton(player.UCID, ClickIds.Menu.Close, 128, 35, 6, 6, "^1X", cancellationToken);

        byte top = 43;

        foreach (var button in page.GetButtons(new MenuContext { Player = player }))
        {
            if (button.TypeIn > 0)
            {
                await _sendInputButton(
                    player.UCID, button.ClickId, 82, top, button.Width, button.Height,
                    button.TypeIn, button.Text, cancellationToken);
            }
            else
            {
                await _sendButton(
                    player.UCID, button.ClickId, 82, top, button.Width, button.Height,
                    button.Text, cancellationToken);
            }

            top += (byte)(button.Height + 2);
        }
    }

    public async Task CloseAsync(Player player, CancellationToken cancellationToken)
    {
        for (byte id = ClickIds.Menu.Background; id <= ClickIds.Jobs.Leave; id++)
        {
            await _deleteButtons(player.UCID, id, id, cancellationToken);
        }
    }
}