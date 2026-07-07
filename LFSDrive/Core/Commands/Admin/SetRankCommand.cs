using LfsCruise.Core.Players;
using LfsCruise.Database;

namespace LfsCruise.Core.Commands.Admin;

public sealed class SetRankCommand : ICommand
{
    private readonly PlayerManager _playerManager;
    private readonly DatabaseService _databaseService;
    private readonly Func<byte, string, CancellationToken, Task> _sendMessage;

    public SetRankCommand(
        PlayerManager playerManager,
        DatabaseService databaseService,
        Func<byte, string, CancellationToken, Task> sendMessage)
    {
        _playerManager = playerManager;
        _databaseService = databaseService;
        _sendMessage = sendMessage;
    }

    public string Name => "setrank";
    public string Description => "Priskiria zaidejui admin ranga.";
    public bool AdminOnly => true;

    public AdminRank RequiredRank => AdminRank.Developer;

    public async Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 2)
        {
            await _sendMessage(
                player.UCID,
                "^1Naudok: ^7!setrank <username> <player|moderator|admin|developer>",
                cancellationToken);
            return;
        }

        var targetUsername = args[0];

        if (!Enum.TryParse<AdminRank>(args[1], true, out var rank))
        {
            await _sendMessage(
                player.UCID,
                "^1Neteisingas rangas. Galimi: ^7player, moderator, admin, developer",
                cancellationToken);
            return;
        }

        await _databaseService.SetAdminLevelAsync(targetUsername, (int)rank, cancellationToken);

        var onlinePlayer = _playerManager.Players.FirstOrDefault(
            p => p.Username.Equals(targetUsername, StringComparison.OrdinalIgnoreCase));

        if (onlinePlayer is not null)
        {
            onlinePlayer.AdminLevel = (int)rank;
        }

        await _sendMessage(
            player.UCID,
            $"^2{targetUsername} rangas pakeistas i: ^7{rank}",
            cancellationToken);
    }
}