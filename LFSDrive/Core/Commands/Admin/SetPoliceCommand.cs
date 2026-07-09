using LfsCruise.Core.Players;
using LfsCruise.Database;

namespace LfsCruise.Core.Commands.Admin;

public sealed class SetPoliceCommand : ICommand
{
    private readonly PlayerManager _playerManager;
    private readonly DatabaseService _databaseService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public SetPoliceCommand(
        PlayerManager playerManager,
        DatabaseService databaseService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _playerManager = playerManager;
        _databaseService = databaseService;
        _sendMessage = sendMessage;
    }

    public string Name => "setpolice";
    public string Description => "Suteikia/atima leidima buti policijos pareigunu.";
    public bool AdminOnly => true;

    public AdminRank RequiredRank => AdminRank.Moderator;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 2 || (args[1].ToLower() != "on" && args[1].ToLower() != "off"))
        {
            await _sendMessage(player.UCID, "^1Naudok: ^7!setpolice <username> <on|off>", cancellationToken);
            return;
        }

        var targetUsername = args[0];
        var authorized = args[1].ToLower() == "on";

        await _databaseService.SetPoliceAuthorizationAsync(targetUsername, authorized, cancellationToken);

        var onlinePlayer = _playerManager.Players.FirstOrDefault(
            p => p.Username.Equals(targetUsername, StringComparison.OrdinalIgnoreCase));

        if (onlinePlayer is not null)
        {
            onlinePlayer.Data.IsPoliceAuthorized = authorized;
        }

        await _sendMessage(
            player.UCID,
            authorized
                ? $"^2{targetUsername} dabar gali buti policijos pareigunu."
                : $"^1{targetUsername} leidimas buti pareigunu panaikintas.",
            cancellationToken);
    }
}