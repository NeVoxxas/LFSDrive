using LfsCruise.Core.Players;

namespace LfsCruise.Core.Commands;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    bool AdminOnly { get; }
    AdminRank RequiredRank => AdminOnly ? AdminRank.Admin : AdminRank.Player;

    Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken);
}