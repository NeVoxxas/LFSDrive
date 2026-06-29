using LfsCruise.Core.Players;

namespace LfsCruise.Core.UI.Menu;

public sealed class MenuRenderer
{
    private readonly Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> _sendButton;
    private readonly Func<byte, byte, byte, CancellationToken, Task> _deleteButtons;

    public MenuRenderer(
        Func<byte, byte, byte, byte, byte, byte, string, CancellationToken, Task> sendButton,
        Func<byte, byte, byte, CancellationToken, Task> deleteButtons)
    {
        _sendButton = sendButton;
        _deleteButtons = deleteButtons;
    }

    public async Task RenderAsync(
        Player player,
        MenuPage page,
        CancellationToken cancellationToken)
    {
        // Background
        await _sendButton(
            player.UCID,
            ClickIds.Menu.Background,
            65,
            30,
            70,
            80,
            "",
            cancellationToken);

        // Title
        await _sendButton(
            player.UCID,
            ClickIds.Menu.Title,
            75,
            35,
            50,
            6,
            $"^2{page.Title}",
            cancellationToken);

        // Close button
        await _sendButton(
            player.UCID,
            ClickIds.Menu.Close,
            128,
            35,
            6,
            6,
            "^1X",
            cancellationToken);

        // Menu buttons
        byte top = 43;

        foreach (var button in page.GetButtons(new MenuContext
        {
            Player = player
        }))
        {
            Console.WriteLine($"MENU DRAW -> {button.Text} ID {button.ClickId}");

            await _sendButton(
                player.UCID,
                button.ClickId,
                82,
                top,
                button.Width,
                button.Height,
                button.Text,
                cancellationToken);

            top += (byte)(button.Height + 2);
        }
    }
    public async Task CloseAsync(Player player, CancellationToken cancellationToken)
    {
        for (byte id = ClickIds.Menu.Background; id <= 199; id++)
        {
            await _deleteButtons(player.UCID, id, id, cancellationToken);
        }
    }
}