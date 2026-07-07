using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands.Admin;

public sealed class PosCommand : ICommand
{
    public bool AdminOnly => true;

    public AdminRank RequiredRank => AdminRank.Moderator;

    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public PosCommand(Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _sendMessage = sendMessage;
    }

    public string Name => "pos";
    public string Description => "Shows your position and speed.";

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        await _sendMessage(
            player.UCID,
            $"^2Pos: ^7X {player.Vehicle.X:F1}, Y {player.Vehicle.Y:F1} | Speed: {player.Vehicle.Speed:F1} km/h",
            cancellationToken
        );
    }
}