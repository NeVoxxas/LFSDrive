using LfsCruise.Core.Players;
using LfsCruise.Core.Vehicles.Tow;

namespace LfsCruise.Core.Commands.Vehicles;

public sealed class TowCommand : ICommand
{
    private readonly TowService _towService;

    public TowCommand(TowService towService)
    {
        _towService = towService;
    }

    public string Name => "tow";
    public string Description => "Iskviecia vilkika - nuveza tave i starto taska uz mokesti.";
    public bool AdminOnly => false;

    public Task ExecuteAsync(Player player, string[] args, CancellationToken cancellationToken)
    {
        return _towService.TowPlayerAsync(player, cancellationToken);
    }
}